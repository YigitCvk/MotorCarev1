import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';

import {
  CreateCustomerRequest,
  CustomerLookupResponse,
  CustomerSummary,
  PagedResult,
  UpdateCustomerRequest
} from '../models/customer.models';

@Injectable({ providedIn: 'root' })
export class CustomersService {
  constructor(private readonly http: HttpClient) {}

  searchCustomers(
    searchText?: string | null,
    pageNumber = 1,
    pageSize = 20
  ): Observable<PagedResult<CustomerLookupResponse>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (searchText?.trim()) {
      params = params.set('q', searchText.trim());
    }

    return this.http
      .get<PagedResult<CustomerLookupResponse>>('/api/customers', { params })
      .pipe(catchError((error) => this.toFriendlyError(error, 'Müşteriler alınamadı.')));
  }

  getById(id: string): Observable<CustomerLookupResponse> {
    return this.http
      .get<CustomerLookupResponse>(`/api/customers/${id}`)
      .pipe(catchError((error) => this.toFriendlyError(error, 'Müşteri bilgisi alınamadı.')));
  }

  getSummary(id: string): Observable<CustomerSummary> {
    return this.http
      .get<CustomerSummary>(`/api/customers/${id}/summary`)
      .pipe(catchError((error) => this.toFriendlyError(error, 'Müşteri özeti alınamadı.')));
  }

  create(request: CreateCustomerRequest): Observable<string> {
    return this.http
      .post<string>('/api/customers', request)
      .pipe(catchError((error) => this.toFriendlyError(error, 'Müşteri kaydedilemedi.')));
  }

  update(id: string, request: UpdateCustomerRequest): Observable<void> {
    return this.http
      .put<void>(`/api/customers/${id}`, request)
      .pipe(catchError((error) => this.toFriendlyError(error, 'Müşteri güncellenemedi.')));
  }

  private toFriendlyError(error: unknown, fallback: string): Observable<never> {
    return throwError(() => new Error(extractApiErrorMessage(error, fallback)));
  }
}

function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const byStatus = statusMessage(error.status);
  const body = error.error;

  if (!body) {
    return byStatus ?? fallback;
  }

  if (typeof body === 'string') {
    return body.trim() || byStatus || fallback;
  }

  const messages: string[] = [];
  for (const key of ['message', 'detail', 'title']) {
    const value = body[key];
    if (typeof value === 'string' && value.trim()) {
      messages.push(value.trim());
    }
  }

  if (body.errors && typeof body.errors === 'object') {
    for (const value of Object.values(body.errors)) {
      if (Array.isArray(value)) {
        messages.push(...value.filter((item): item is string => typeof item === 'string'));
      }
    }
  }

  return messages.join(' ') || byStatus || fallback;
}

function statusMessage(status: number): string | null {
  switch (status) {
    case 401:
      return 'Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın.';
    case 403:
      return 'Bu işlem için yetkiniz bulunmuyor.';
    case 404:
      return 'İstenen müşteri kaydı bulunamadı.';
    case 409:
      return 'Bu bilgiler mevcut bir müşteri kaydıyla çakışıyor.';
    case 422:
      return 'Formdaki bilgileri kontrol edip tekrar deneyin.';
    default:
      return null;
  }
}
