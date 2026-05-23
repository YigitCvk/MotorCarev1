import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService, QueryValue } from '../../../core/api/api.service';
import {
  AddConsumableToOrderRequest,
  AddOperationToOrderRequest,
  AddPartToOrderRequest,
  AddPaymentToOrderRequest,
  CreateServiceOrderRequest,
  CustomerLookup,
  PagedResult,
  ServiceOrder,
  ServiceOrderListQuery,
  SetOrderDiscountRequest,
  UpdateServiceOrderStatusRequest,
  VehicleLookup
} from '../models/service-order.models';

@Injectable({ providedIn: 'root' })
export class ServiceOrdersApiService {
  private readonly serviceOrdersUrl = '/api/service-orders';
  private readonly customersUrl = '/api/customers';

  constructor(private readonly api: ApiService) {}

  getServiceOrders(query: ServiceOrderListQuery): Observable<PagedResult<ServiceOrder>> {
    return this.api.get<PagedResult<ServiceOrder>>(this.serviceOrdersUrl, this.cleanQuery({
      pageNumber: query.pageNumber ?? 1,
      pageSize: query.pageSize ?? 20,
      customerId: query.customerId,
      status: query.status,
      q: query.q,
      openedFrom: query.openedFrom,
      openedTo: query.openedTo
    }));
  }

  getServiceOrder(id: string): Observable<ServiceOrder> {
    return this.api.get<ServiceOrder>(`${this.serviceOrdersUrl}/${id}`);
  }

  createServiceOrder(request: CreateServiceOrderRequest): Observable<string> {
    return this.api.post<string>(this.serviceOrdersUrl, request);
  }

  addOperation(id: string, request: AddOperationToOrderRequest): Observable<void> {
    return this.api.post<void>(`${this.serviceOrdersUrl}/${id}/operations`, request);
  }

  addPart(id: string, request: AddPartToOrderRequest): Observable<void> {
    return this.api.post<void>(`${this.serviceOrdersUrl}/${id}/parts`, request);
  }

  addConsumable(id: string, request: AddConsumableToOrderRequest): Observable<void> {
    return this.api.post<void>(`${this.serviceOrdersUrl}/${id}/consumables`, request);
  }

  addPayment(id: string, request: AddPaymentToOrderRequest): Observable<void> {
    return this.api.post<void>(`${this.serviceOrdersUrl}/${id}/payments`, request);
  }

  setDiscount(id: string, request: SetOrderDiscountRequest): Observable<void> {
    return this.api.patch<void>(`${this.serviceOrdersUrl}/${id}/discount`, request);
  }

  updateStatus(id: string, request: UpdateServiceOrderStatusRequest): Observable<void> {
    return this.api.put<void>(`${this.serviceOrdersUrl}/${id}/status`, request);
  }

  removeOperation(id: string, operationId: string): Observable<void> {
    return this.api.delete<void>(`${this.serviceOrdersUrl}/${id}/operations/${operationId}`);
  }

  removePart(id: string, partId: string): Observable<void> {
    return this.api.delete<void>(`${this.serviceOrdersUrl}/${id}/parts/${partId}`);
  }

  removeConsumable(id: string, consumableId: string): Observable<void> {
    return this.api.delete<void>(`${this.serviceOrdersUrl}/${id}/consumables/${consumableId}`);
  }

  searchCustomers(query: string): Observable<PagedResult<CustomerLookup>> {
    return this.api.get<PagedResult<CustomerLookup>>(this.customersUrl, {
      q: query,
      pageNumber: 1,
      pageSize: 20
    });
  }

  getCustomerVehicles(customerId: string): Observable<VehicleLookup[]> {
    return this.api.get<VehicleLookup[]>(`${this.customersUrl}/${customerId}/vehicles`);
  }

  private cleanQuery(query: Record<string, QueryValue>): Record<string, QueryValue> {
    return Object.fromEntries(Object.entries(query).filter(([, value]) => value !== null && value !== undefined && value !== ''));
  }
}
