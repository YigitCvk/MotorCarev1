'use client';

import { useState, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Upload,
  CheckCircle2,
  XCircle,
  AlertTriangle,
  FileText,
  Clock,
  ChevronDown,
  ChevronUp,
  ChevronLeft,
  ChevronRight,
  Download,
} from 'lucide-react';
import { toast } from 'sonner';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { friendlyError } from '@/core/api/errors';
import { downloadFile } from '@/shared/utils/download';
import { dateTimeText } from '@/shared/utils/format';

// ─── DTOs ────────────────────────────────────────────────────────────────────

interface ImportBatchRowDto {
  id: string;
  rowNumber: number;
  status: string;
  rawJson: string;
  normalizedJson: string | null;
  errorMessage: string | null;
  warningMessage: string | null;
}

interface ImportBatchDto {
  id: string;
  importType: string;
  originalFileName: string;
  status: string;
  totalRows: number;
  validRows: number;
  warningRows: number;
  errorRows: number;
  importedRows: number;
  skippedRows: number;
  createdAtUtc: string;
  completedAtUtc: string | null;
  previewRows: ImportBatchRowDto[];
}

// ─── Constants ────────────────────────────────────────────────────────────────

const IMPORT_TYPES: Array<{ value: string; label: string; description: string }> = [
  { value: 'Customers', label: 'Müşteriler', description: 'Müşteri listesini CSV olarak içe aktarın' },
  { value: 'Vehicles', label: 'Araçlar', description: 'Araç listesini CSV olarak içe aktarın' },
  { value: 'ServiceHistory', label: 'Servis Geçmişi', description: 'Servis kayıtlarını CSV olarak içe aktarın' },
];

const MAX_CSV_SIZE_BYTES = 5 * 1024 * 1024;
const MAX_IMPORT_ROWS = 10_000;
const ROWS_PER_PAGE = 100;
const ACCEPTED_CSV_MIME_TYPES = new Set([
  'text/csv',
  'application/csv',
  'application/vnd.ms-excel',
  'text/plain',
  'application/octet-stream',
]);

const TEMPLATE_FILE_NAMES: Record<string, string> = {
  Customers: 'musteriler-sablonu.csv',
  Vehicles: 'araclar-sablonu.csv',
  ServiceHistory: 'servis-gecmisi-sablonu.csv',
};

const STATUS_LABEL: Record<string, string> = {
  Pending: 'Bekliyor',
  Processing: 'İşleniyor',
  Preview: 'Ön İzleme',
  Committed: 'Tamamlandı',
  Failed: 'Başarısız',
  Cancelled: 'İptal',
};

function statusIcon(status: string) {
  switch (status) {
    case 'Committed': return <CheckCircle2 size={15} className="text-green-600" />;
    case 'Failed': return <XCircle size={15} className="text-red-500" />;
    case 'Preview': return <FileText size={15} className="text-blue-500" />;
    case 'Processing': return <Clock size={15} className="text-amber-500 animate-pulse" />;
    default: return <Clock size={15} className="text-slate-400" />;
  }
}

function rowStatusBadge(status: string) {
  switch (status) {
    case 'Valid': return <span className="badge-green text-xs">Geçerli</span>;
    case 'Warning': return <span className="badge-yellow text-xs">Uyarı</span>;
    case 'Error': return <span className="badge-red text-xs">Hata</span>;
    case 'Imported': return <span className="badge-green text-xs">Aktarıldı</span>;
    case 'Skipped': return <span className="badge-gray text-xs">Atlandı</span>;
    default: return <span className="badge-gray text-xs">{status}</span>;
  }
}

function parseRowLabel(rawJson: string): string {
  try {
    const obj = JSON.parse(rawJson) as Record<string, unknown>;
    const name = String(obj['FullName'] ?? obj['CustomerName'] ?? obj['Name'] ?? obj['Plate'] ?? obj['plate'] ?? '');
    return name || rawJson.slice(0, 60);
  } catch {
    return rawJson.slice(0, 60);
  }
}

function formatJson(value: string): string {
  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
}

function isProcessingStatus(status?: string): boolean {
  return status === 'Pending' || status === 'Processing';
}

function validateCsvFile(file: File): string | null {
  if (!file.name.toLowerCase().endsWith('.csv')) {
    return 'Yalnızca .csv uzantılı dosyalar yüklenebilir.';
  }

  if (file.size === 0) {
    return 'Boş dosya yüklenemez.';
  }

  if (file.size > MAX_CSV_SIZE_BYTES) {
    return 'CSV dosyası en fazla 5 MB olabilir.';
  }

  const mimeType = file.type.toLowerCase().split(';')[0].trim();
  if (mimeType && !ACCEPTED_CSV_MIME_TYPES.has(mimeType)) {
    return 'Dosya türü CSV olarak tanınmadı. Geçerli bir CSV dosyası seçin.';
  }

  return null;
}

function QueryError({ message, onRetry }: { message: string; onRetry: () => void }) {
  return (
    <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">
      <p>{message}</p>
      <button type="button" onClick={onRetry} className="mt-2 font-medium underline">
        Tekrar dene
      </button>
    </div>
  );
}

// ─── Component ────────────────────────────────────────────────────────────────

export default function ImportsPage() {
  const qc = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [selectedImportType, setSelectedImportType] = useState('Customers');
  const [uploadError, setUploadError] = useState('');
  const [commitError, setCommitError] = useState('');
  const [isTemplateDownloading, setIsTemplateDownloading] = useState(false);
  const [activeBatchId, setActiveBatchId] = useState<string | null>(null);
  const [expandedBatchId, setExpandedBatchId] = useState<string | null>(null);
  const [showAllRows, setShowAllRows] = useState(false);
  const [rowsPage, setRowsPage] = useState(1);

  // ── Fetch all batches ──────────────────────────────────────────────────────

  const {
    data: batches,
    isLoading: isBatchesLoading,
    isError: isBatchesError,
    isFetching: isBatchesFetching,
    refetch: refetchBatches,
  } = useQuery<ImportBatchDto[]>({
    queryKey: ['import-batches'],
    queryFn: async () => {
      const { data } = await apiClient.get<ImportBatchDto[]>('/api/imports');
      return data;
    },
    staleTime: 15_000,
    retry: 1,
    refetchInterval: (query) => {
      const currentBatches = query.state.data;
      return currentBatches?.some((batch) => isProcessingStatus(batch.status)) ? 3_000 : false;
    },
  });

  // ── Active batch detail ────────────────────────────────────────────────────

  const {
    data: activeBatch,
    isLoading: isActiveBatchLoading,
    isError: isActiveBatchError,
    isFetching: isActiveBatchFetching,
    refetch: refetchActiveBatch,
  } = useQuery<ImportBatchDto | null>({
    queryKey: ['import-batch', activeBatchId],
    queryFn: async () => {
      if (!activeBatchId) return null;
      const { data } = await apiClient.get<ImportBatchDto>(`/api/imports/${activeBatchId}?previewRows=50`);
      return data;
    },
    enabled: !!activeBatchId,
    staleTime: 15_000,
    retry: 1,
    refetchInterval: (query) => (
      isProcessingStatus(query.state.data?.status) ? 3_000 : false
    ),
  });

  const fullRowsQuery = useQuery<ImportBatchRowDto[]>({
    queryKey: ['import-batch-rows', activeBatchId],
    queryFn: async () => {
      if (!activeBatchId) return [];
      const { data } = await apiClient.get<ImportBatchRowDto[]>(
        `/api/imports/${activeBatchId}/rows`,
        { params: { maxRows: MAX_IMPORT_ROWS } },
      );
      return data;
    },
    enabled: !!activeBatchId && showAllRows,
    staleTime: 15_000,
    retry: 1,
    refetchInterval: () => (
      isProcessingStatus(activeBatch?.status) ? 3_000 : false
    ),
  });

  // ── Upload mutation ────────────────────────────────────────────────────────

  const uploadMutation = useMutation({
    mutationFn: async ({ file, importType }: { file: File; importType: string }) => {
      const formData = new FormData();
      formData.append('file', file);
      const { data } = await apiClient.post<ImportBatchDto>(
        `/api/imports/upload?importType=${importType}`,
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } }
      );
      return data;
    },
    onSuccess: (batch) => {
      setActiveBatchId(batch.id);
      setShowAllRows(false);
      setRowsPage(1);
      setUploadError('');
      setCommitError('');
      void qc.invalidateQueries({ queryKey: ['import-batches'] });
      void qc.invalidateQueries({ queryKey: ['import-batch', batch.id] });
      toast.success(
        isProcessingStatus(batch.status)
          ? 'Dosya yüklendi ve işleniyor.'
          : 'Dosya yüklendi. Ön izleme hazır.',
      );
    },
    onError: (err: unknown) => {
      const msg = friendlyError(err, 'Dosya yüklenemedi.');
      setUploadError(msg);
      toast.error(msg);
    },
  });

  // ── Commit mutation ────────────────────────────────────────────────────────

  const commitMutation = useMutation({
    mutationFn: async (batchId: string) => {
      const { data } = await apiClient.post<ImportBatchDto>(`/api/imports/${batchId}/commit`);
      return data;
    },
    onSuccess: (batch) => {
      setCommitError('');
      void qc.invalidateQueries({ queryKey: ['import-batches'] });
      void qc.invalidateQueries({ queryKey: ['import-batch', batch.id] });
      void qc.invalidateQueries({ queryKey: ['import-batch-rows', batch.id] });
      toast.success(`İçe aktarım tamamlandı: ${batch.importedRows} kayıt eklendi.`);
    },
    onError: (err: unknown) => {
      const message = friendlyError(err, 'İçe aktarım tamamlanamadı.');
      setCommitError(message);
      toast.error(message);
    },
  });

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;

    const validationError = validateCsvFile(file);
    if (validationError) {
      setUploadError(validationError);
      toast.error(validationError);
      e.target.value = '';
      return;
    }

    setUploadError('');
    setCommitError('');
    uploadMutation.mutate({ file, importType: selectedImportType });
    e.target.value = '';
  }

  async function handleTemplateDownload() {
    setIsTemplateDownloading(true);
    try {
      await downloadFile(`/api/imports/templates/${encodeURIComponent(selectedImportType)}`, {
        fallbackFileName: TEMPLATE_FILE_NAMES[selectedImportType] ?? 'import-sablonu.csv',
      });
    } catch (err: unknown) {
      toast.error(friendlyError(err, 'İçe aktarım şablonu indirilemedi.'));
    } finally {
      setIsTemplateDownloading(false);
    }
  }

  function openBatch(batchId: string) {
    setActiveBatchId(batchId);
    setExpandedBatchId(null);
    setShowAllRows(false);
    setRowsPage(1);
    setCommitError('');
  }

  function closeBatch() {
    setActiveBatchId(null);
    setShowAllRows(false);
    setRowsPage(1);
    setCommitError('');
  }

  const displayBatch = activeBatch ?? batches?.find((b) => b.id === activeBatchId);
  const visibleRows = showAllRows
    ? (fullRowsQuery.data ?? []).slice((rowsPage - 1) * ROWS_PER_PAGE, rowsPage * ROWS_PER_PAGE)
    : (displayBatch?.previewRows ?? []).slice(0, 20);
  const totalRowsForPaging = fullRowsQuery.data?.length ?? 0;
  const totalRowPages = Math.max(1, Math.ceil(totalRowsForPaging / ROWS_PER_PAGE));

  return (
    <div>
      <PageHeader title="İçeri Aktarım" subtitle="CSV dosyası yükleyerek toplu veri aktarımı yapın" />

      {/* Upload card */}
      <div className="card p-6 mb-6 max-w-2xl">
        <h2 className="font-semibold text-slate-900 mb-4">Yeni Aktarım</h2>

        <div className="form-group mb-4">
          <label className="label">Veri Türü</label>
          <div className="grid sm:grid-cols-3 gap-3">
            {IMPORT_TYPES.map((t) => (
              <button
                key={t.value}
                type="button"
                onClick={() => setSelectedImportType(t.value)}
                className={[
                  'text-left p-3 rounded-lg border-2 transition-colors',
                  selectedImportType === t.value
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-slate-200 hover:border-slate-300',
                ].join(' ')}
              >
                <p className="text-sm font-medium text-slate-900">{t.label}</p>
                <p className="text-xs text-slate-400 mt-0.5">{t.description}</p>
              </button>
            ))}
          </div>
        </div>

        <div className="flex items-center gap-3">
          <input
            ref={fileInputRef}
            type="file"
            accept=".csv,text/csv,application/csv,application/vnd.ms-excel"
            className="hidden"
            onChange={handleFileChange}
          />
          <button
            onClick={() => fileInputRef.current?.click()}
            disabled={uploadMutation.isPending}
            className="btn-primary flex items-center gap-2"
          >
            <Upload size={15} />
            {uploadMutation.isPending ? 'Yükleniyor...' : 'CSV Dosyası Seç'}
          </button>
          <button
            type="button"
            onClick={() => void handleTemplateDownload()}
            disabled={isTemplateDownloading}
            className="btn-secondary flex items-center gap-2 text-sm"
          >
            <Download size={14} />
            {isTemplateDownloading ? 'İndiriliyor...' : 'Şablon İndir'}
          </button>
        </div>

        <p className="mt-2 text-xs text-slate-400">En fazla 5 MB boyutunda CSV dosyası yükleyebilirsiniz.</p>
        {uploadError && <p className="error-text mt-2">{uploadError}</p>}
      </div>

      {/* Active batch preview */}
      {activeBatchId && (
        <div className="card p-6 mb-6 max-w-4xl">
          <div className="flex items-center justify-between gap-4 mb-4">
            <div>
              <h2 className="font-semibold text-slate-900">Ön İzleme</h2>
              {displayBatch && (
                <p className="text-sm text-slate-500 mt-0.5">
                  {displayBatch.originalFileName} · {displayBatch.totalRows} satır
                </p>
              )}
            </div>
            <button onClick={closeBatch} className="btn-ghost text-sm">Kapat</button>
          </div>

          {isActiveBatchLoading && <PageLoading />}

          {isActiveBatchError && (
            <QueryError
              message="Aktarım ön izlemesi yüklenemedi."
              onRetry={() => void refetchActiveBatch()}
            />
          )}

          {!isActiveBatchError && displayBatch && (
            <>
              {isProcessingStatus(displayBatch.status) && (
                <div className="mb-4 flex items-center gap-2 rounded-lg bg-amber-50 p-3 text-sm text-amber-700">
                  <Clock size={15} className="animate-pulse" />
                  Dosya işleniyor. Ön izleme hazır olduğunda bu alan otomatik güncellenecek.
                </div>
              )}

              {/* Stats */}
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-4">
                {[
                  { label: 'Geçerli', value: displayBatch.validRows, color: 'text-green-700' },
                  { label: 'Uyarı', value: displayBatch.warningRows, color: 'text-amber-600' },
                  { label: 'Hata', value: displayBatch.errorRows, color: 'text-red-600' },
                  { label: 'Toplam', value: displayBatch.totalRows, color: 'text-slate-700' },
                ].map((s) => (
                  <div key={s.label} className="rounded-lg bg-slate-50 p-3 text-center">
                    <p className={`text-xl font-bold ${s.color}`}>{s.value}</p>
                    <p className="text-xs text-slate-400 mt-0.5">{s.label}</p>
                  </div>
                ))}
              </div>

              {/* Preview rows */}
              {displayBatch.totalRows > 0 && (
                <div className="mb-4">
                  <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
                    <p className="text-sm font-medium text-slate-700">
                      {showAllRows ? 'Tüm Satırlar' : 'Ön İzleme Satırları'}
                    </p>
                    <button
                      type="button"
                      onClick={() => {
                        setShowAllRows((current) => !current);
                        setRowsPage(1);
                      }}
                      className="btn-secondary text-sm"
                    >
                      {showAllRows ? 'Ön İzlemeye Dön' : 'Tüm Satırları İncele'}
                    </button>
                  </div>

                  {showAllRows && fullRowsQuery.isLoading && <PageLoading />}

                  {showAllRows && fullRowsQuery.isError && (
                    <QueryError
                      message="Aktarım satırları yüklenemedi."
                      onRetry={() => void fullRowsQuery.refetch()}
                    />
                  )}

                  {(!showAllRows || fullRowsQuery.isSuccess) && visibleRows.length > 0 && (
                    <div className="card p-0 mb-3 overflow-x-auto">
                      <table className="table min-w-[760px]">
                        <thead>
                          <tr>
                            <th className="w-12">#</th>
                            <th>Veri</th>
                            <th className="w-24">Durum</th>
                            <th>Mesaj</th>
                          </tr>
                        </thead>
                        <tbody>
                          {visibleRows.map((row) => (
                            <tr key={row.id}>
                              <td className="text-slate-400 text-xs">{row.rowNumber}</td>
                              <td className="text-sm text-slate-700">
                                <details className="max-w-xl">
                                  <summary className="cursor-pointer font-mono">
                                    {parseRowLabel(row.rawJson)}
                                  </summary>
                                  <div className="mt-2 space-y-2">
                                    <div>
                                      <p className="mb-1 text-xs font-semibold text-slate-500">Ham veri</p>
                                      <pre className="max-h-52 overflow-auto whitespace-pre-wrap rounded bg-slate-50 p-2 text-xs">
                                        {formatJson(row.rawJson)}
                                      </pre>
                                    </div>
                                    {row.normalizedJson && (
                                      <div>
                                        <p className="mb-1 text-xs font-semibold text-slate-500">Normalize veri</p>
                                        <pre className="max-h-52 overflow-auto whitespace-pre-wrap rounded bg-slate-50 p-2 text-xs">
                                          {formatJson(row.normalizedJson)}
                                        </pre>
                                      </div>
                                    )}
                                  </div>
                                </details>
                              </td>
                              <td>{rowStatusBadge(row.status)}</td>
                              <td className="text-xs text-slate-500">
                                {row.errorMessage ?? row.warningMessage ?? '—'}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  )}

                  {showAllRows && fullRowsQuery.isSuccess && totalRowsForPaging === 0 && (
                    <p className="rounded-lg bg-slate-50 p-4 text-sm text-slate-500">
                      Bu aktarım için satır bulunamadı.
                    </p>
                  )}

                  {showAllRows && fullRowsQuery.isSuccess && totalRowsForPaging > ROWS_PER_PAGE && (
                    <div className="flex items-center justify-end gap-3 text-sm text-slate-500">
                      <button
                        type="button"
                        onClick={() => setRowsPage((page) => Math.max(1, page - 1))}
                        disabled={rowsPage === 1}
                        className="btn-secondary p-2"
                        aria-label="Önceki sayfa"
                      >
                        <ChevronLeft size={14} />
                      </button>
                      <span>{rowsPage} / {totalRowPages}</span>
                      <button
                        type="button"
                        onClick={() => setRowsPage((page) => Math.min(totalRowPages, page + 1))}
                        disabled={rowsPage === totalRowPages}
                        className="btn-secondary p-2"
                        aria-label="Sonraki sayfa"
                      >
                        <ChevronRight size={14} />
                      </button>
                    </div>
                  )}
                </div>
              )}

              {/* Commit / cancel */}
              {displayBatch.status === 'Preview' && (
                <>
                  <div className="flex flex-wrap items-center gap-3">
                    {displayBatch.errorRows > 0 && (
                      <div className="flex items-center gap-1.5 text-sm text-amber-700">
                        <AlertTriangle size={14} />
                        {displayBatch.errorRows} satır hatalı — aktarımda atlanacak.
                      </div>
                    )}
                    <div className="ml-auto flex gap-2">
                      <button
                        onClick={closeBatch}
                        className="btn-ghost text-sm"
                      >
                        Vazgeç
                      </button>
                      <button
                        onClick={() => {
                          setCommitError('');
                          commitMutation.mutate(displayBatch.id);
                        }}
                        disabled={commitMutation.isPending || displayBatch.validRows + displayBatch.warningRows === 0}
                        className="btn-primary text-sm"
                      >
                        {commitMutation.isPending
                          ? 'Aktarılıyor...'
                          : `${displayBatch.validRows + displayBatch.warningRows} Kaydı Aktar`}
                      </button>
                    </div>
                  </div>
                  {commitError && <p className="error-text mt-2">{commitError}</p>}
                </>
              )}

              {displayBatch.status === 'Committed' && (
                <div className="flex items-center gap-2 text-sm text-green-700">
                  <CheckCircle2 size={16} />
                  {displayBatch.importedRows} kayıt başarıyla aktarıldı.
                  {displayBatch.skippedRows > 0 && ` (${displayBatch.skippedRows} atlandı)`}
                </div>
              )}

              {displayBatch.status === 'Failed' && (
                <div className="flex items-center gap-2 text-sm text-red-700">
                  <XCircle size={16} />
                  Aktarım işlenemedi. Dosyayı kontrol edip yeniden yükleyin.
                </div>
              )}

              {(isActiveBatchFetching || fullRowsQuery.isFetching) && !isActiveBatchLoading && (
                <p className="mt-3 text-xs text-slate-400">Aktarım durumu güncelleniyor...</p>
              )}
            </>
          )}
        </div>
      )}

      {/* Past batches */}
      <div className="max-w-4xl">
        <div className="mb-3 flex items-center justify-between gap-3">
          <h2 className="font-semibold text-slate-900">Geçmiş Aktarımlar</h2>
          {isBatchesFetching && !isBatchesLoading && (
            <span className="text-xs text-slate-400">Güncelleniyor...</span>
          )}
        </div>

        {isBatchesLoading ? (
          <PageLoading />
        ) : isBatchesError ? (
          <QueryError
            message="Geçmiş aktarımlar yüklenemedi."
            onRetry={() => void refetchBatches()}
          />
        ) : !batches || batches.length === 0 ? (
          <div className="card p-8 text-center text-sm text-slate-400">
            Henüz içe aktarım yapılmamış.
          </div>
        ) : (
          <div className="space-y-2">
            {batches.map((batch) => (
              <div key={batch.id} className="card p-0 overflow-hidden">
                <button
                  onClick={() => setExpandedBatchId(expandedBatchId === batch.id ? null : batch.id)}
                  className="w-full flex items-center justify-between px-5 py-3 hover:bg-slate-50 transition-colors"
                >
                  <div className="flex items-center gap-3 min-w-0">
                    {statusIcon(batch.status)}
                    <div className="text-left min-w-0">
                      <p className="text-sm font-medium text-slate-900 truncate">{batch.originalFileName}</p>
                      <p className="text-xs text-slate-400">
                        {IMPORT_TYPES.find((t) => t.value === batch.importType)?.label ?? batch.importType}
                        {' · '}{dateTimeText(batch.createdAtUtc)}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4 shrink-0">
                    <div className="text-right text-xs text-slate-500 hidden sm:block">
                      <p>{batch.totalRows} satır</p>
                      <p className="text-green-600">{batch.importedRows} aktarıldı</p>
                    </div>
                    <span className="text-xs text-slate-500">{STATUS_LABEL[batch.status] ?? batch.status}</span>
                    {expandedBatchId === batch.id ? <ChevronUp size={14} className="text-slate-400" /> : <ChevronDown size={14} className="text-slate-400" />}
                  </div>
                </button>

                {expandedBatchId === batch.id && (
                  <div className="border-t border-slate-100 px-5 py-4">
                    <div className="grid grid-cols-4 gap-3 mb-3 text-center text-xs">
                      <div><p className="font-semibold text-green-700">{batch.validRows}</p><p className="text-slate-400">Geçerli</p></div>
                      <div><p className="font-semibold text-amber-600">{batch.warningRows}</p><p className="text-slate-400">Uyarı</p></div>
                      <div><p className="font-semibold text-red-600">{batch.errorRows}</p><p className="text-slate-400">Hata</p></div>
                      <div><p className="font-semibold text-blue-700">{batch.importedRows}</p><p className="text-slate-400">Aktarıldı</p></div>
                    </div>
                    {batch.status === 'Preview' && (
                      <div className="flex gap-2 mt-2">
                        <button
                          onClick={() => openBatch(batch.id)}
                          className="btn-secondary text-sm"
                        >
                          Ön İzlemeyi Aç
                        </button>
                        <button
                          onClick={() => {
                            openBatch(batch.id);
                            commitMutation.mutate(batch.id);
                          }}
                          disabled={commitMutation.isPending}
                          className="btn-primary text-sm"
                        >
                          {commitMutation.isPending ? 'Aktarılıyor...' : 'Aktar'}
                        </button>
                      </div>
                    )}
                    {isProcessingStatus(batch.status) && (
                      <div className="mt-2 flex items-center gap-2 text-sm text-amber-700">
                        <Clock size={14} className="animate-pulse" />
                        İşleniyor, durum otomatik güncellenecek.
                      </div>
                    )}
                    {batch.status !== 'Preview' && (
                      <button
                        type="button"
                        onClick={() => openBatch(batch.id)}
                        className="btn-secondary mt-2 text-sm"
                      >
                        Ayrıntıları Aç
                      </button>
                    )}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
