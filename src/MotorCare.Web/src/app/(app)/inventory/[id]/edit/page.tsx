import { redirect } from 'next/navigation';

export default async function InventoryEditAliasPage({
  params,
}: {
  params: Promise<{ id: string }>;
}): Promise<never> {
  const { id } = await params;
  redirect(`/inventory/${id}`);
}
