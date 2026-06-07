/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

/** @format int32 */
export enum Gender {
  Value1 = 1,
  Value2 = 2,
  Value3 = 3,
}

/** @format int32 */
export enum AddressType {
  Value1 = 1,
  Value2 = 2,
  Value3 = 3,
  Value4 = 4,
  Value5 = 5,
}

export interface Address {
  /** @format int32 */
  id?: number;
  /** @format uuid */
  addressId?: string;
  addressType?: AddressType;
  streetAddress?: string | null;
  city?: string | null;
  /** @format int32 */
  postCode?: number | null;
  state?: string | null;
  countryCode?: string | null;
  country?: string | null;
  default?: boolean | null;
  /** @format int32 */
  buyerCode?: number;
}

export interface AddressAPIModel {
  /** @format int32 */
  id?: number;
  /** @format uuid */
  addressId?: string;
  addressType?: AddressType;
  streetAddress?: string | null;
  city?: string | null;
  /** @format int32 */
  postCode?: number | null;
  state?: string | null;
  countryCode?: string | null;
  country?: string | null;
  default?: boolean | null;
  /** @format int32 */
  buyerCode?: number | null;
}

export interface AddressAPIModelPaginationAPIModel {
  items?: AddressAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface BadRequestResult {
  /** @format int32 */
  statusCode?: number;
}

export interface BankAPIModel {
  name?: string | null;
  bankCode?: string | null;
  swiftCode?: string | null;
  telephoneNos?: string | null;
  currencyCode?: string | null;
  /** @format int32 */
  addressId?: number | null;
  /** @format double */
  loanLimit?: number;
}

export interface BankAPIModelPaginationAPIModel {
  items?: BankAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface BasisAPIModel {
  /** @format int32 */
  id?: number;
  code?: string | null;
  description?: string | null;
  valueAdd?: boolean;
}

export interface BasisAPIModelPaginationAPIModel {
  items?: BasisAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface BuyerAPIModel {
  /** @format int32 */
  buyerCode?: number;
  status?: string | null;
  name?: string | null;
  telephoneNos?: string | null;
  mobileNos?: string | null;
  /** @format uuid */
  addressId?: string | null;
  fax?: string | null;
  cusdec?: string | null;
  addresses?: Address[] | null;
}

export interface BuyerAPIModelPaginationAPIModel {
  items?: BuyerAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface Country {
  /** @format int32 */
  id?: number;
  code?: string | null;
  name?: string | null;
  /** @format byte */
  flag?: string | null;
}

export interface CountryAPIModel {
  /** @format int32 */
  id?: number;
  code?: string | null;
  name?: string | null;
  /** @format byte */
  flag?: string | null;
}

export interface CountryAPIModelPaginationAPIModel {
  items?: CountryAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface CreateAddressAPIModel {
  /** @format int32 */
  id?: number;
  /** @format uuid */
  addressId?: string;
  addressType?: AddressType;
  streetAddress?: string | null;
  city?: string | null;
  /** @format int32 */
  postCode?: number | null;
  state?: string | null;
  countryCode?: string | null;
  /** @default false */
  default?: boolean | null;
}

export interface CreateBankAPIModel {
  name?: string | null;
  bankCode?: string | null;
  swiftCode?: string | null;
  telephoneNos?: string | null;
  currencyCode?: string | null;
  /** @format int32 */
  addressId?: number | null;
  /** @format double */
  loanLimit?: number;
}

export interface CreateBuyerAPIModel {
  /** @format int32 */
  buyerCode?: number;
  status?: string | null;
  /** @format uuid */
  addreessId?: string | null;
  name?: string | null;
  telephoneNos?: string | null;
  mobileNos?: string | null;
  addressId?: string | null;
  fax?: string | null;
  cusdec?: string | null;
}

export interface CreateCountryAPIModel {
  /** @format int32 */
  id?: number;
  code?: string | null;
  name?: string | null;
  /** @format byte */
  flag?: string | null;
}

export interface CreateCurrencyAPIModel {
  code?: string | null;
  name?: string | null;
  countryCode?: string | null;
  minor?: string | null;
}

export interface CreateCurrencyExchangeAPIModel {
  /** @format int32 */
  id?: number;
  baseCurrency?: string | null;
  quoteCurrency?: string | null;
  /** @format double */
  rate?: number | null;
  /** @format date-time */
  exchangeDate?: string;
}

export interface CreateFeatureAPIModel {
  /** @format int32 */
  id?: number;
  description?: string | null;
}

export interface CreatePortDestinationAPIModel {
  /** @format int32 */
  id?: number;
  countryCode?: string | null;
  destinationName?: string | null;
}

export interface CreateStyleDetailsAPIModel {
  /** @format int32 */
  buyerCode?: number;
  order?: string | null;
  /** @format date-time */
  orderDate?: string;
  /** @format int32 */
  typeCode?: number;
  styleCode?: string | null;
  unit?: string | null;
  /** @format double */
  quantity?: number | null;
  /** @format double */
  unitPrice?: number | null;
}

export interface CreateSupplierAPIModel {
  /** @format int32 */
  supplierCode?: number;
  name?: string | null;
  telephoneNos?: string | null;
  mobileNos?: string | null;
  fax?: string | null;
  /** @format uuid */
  addressId?: string | null;
  addresses?: Address[] | null;
}

export interface CreateUnitAPIModel {
  /** @format int32 */
  id?: number;
  code?: string | null;
  description?: string | null;
}

export interface CurrencyAPIModel {
  /** @format int32 */
  id?: number;
  code?: string | null;
  name?: string | null;
  countryCode?: string | null;
  country?: Country;
  minor?: string | null;
  currencyDetails?: string | null;
}

export interface CurrencyAPIModelPaginationAPIModel {
  items?: CurrencyAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface CurrencyExchangeAPIModel {
  /** @format int32 */
  id?: number;
  baseCurrency?: string | null;
  quoteCurrency?: string | null;
  /** @format double */
  rate?: number | null;
  /** @format date-time */
  exchangeDate?: string;
}

export interface CurrencyExchangeAPIModelPaginationAPIModel {
  items?: CurrencyExchangeAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface FeatureAPIModel {
  /** @format int32 */
  id?: number;
  description?: string | null;
}

export interface GarmentType {
  /** @format int32 */
  id?: number;
  typeName?: string | null;
}

export interface GarmentTypeAPIModel {
  /** @format int32 */
  id?: number;
  typeName?: string | null;
}

export interface GarmentTypeAPIModelPaginationAPIModel {
  items?: GarmentTypeAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface LoginUserAPIModel {
  email?: string | null;
  password?: string | null;
}

export interface NoContentResult {
  /** @format int32 */
  statusCode?: number;
}

export interface POAPIModel {
  /** @format int32 */
  buyerCode?: number;
  buyer?: string | null;
  order?: string | null;
  /** @format date-time */
  orderDate?: string;
  /** @format int32 */
  garmentType?: number;
  garmentTypeName?: string | null;
  type?: GarmentType;
  countryCode?: string | null;
  unitCode?: string | null;
  /** @format double */
  totalQuantity?: number;
  currencyCode?: string | null;
  season?: string | null;
  basisCode?: string | null;
  /** @format double */
  basisValue?: number;
}

export interface POAPIModelPaginationAPIModel {
  items?: POAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface PortDestinationAPIModel {
  /** @format int32 */
  id?: number;
  countryCode?: string | null;
  destinationName?: string | null;
}

export interface PortDestinationAPIModelPaginationAPIModel {
  items?: PortDestinationAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface RegisterUserAPIModel {
  email?: string | null;
  password?: string | null;
  knownAs?: string | null;
  gender?: string | null;
  phoneNumber?: string | null;
  /** @format date-time */
  dateOfBirth?: string | null;
  city?: string | null;
  country?: string | null;
}

export interface RegisteredUserAPIModel {
  email?: string | null;
  token?: string | null;
  knownAs?: string | null;
  /** @format byte */
  photo?: string | null;
  success?: boolean;
  refreshToken?: string | null;
  /** @format date-time */
  refreshTokenExpiry?: string;
}

export interface StyleAPIModel {
  /** @format int32 */
  id?: number;
  /** @format int32 */
  buyerCode?: number;
  buyer?: string | null;
  order?: string | null;
  /** @format date-time */
  orderDate?: string;
  /** @format int32 */
  typeCode?: number;
  type?: string | null;
  styleCode?: string | null;
  unit?: string | null;
  /** @format double */
  quantity?: number | null;
  /** @format double */
  unitPrice?: number | null;
  /** @format double */
  exportBalance?: number | null;
  supplierReturn?: boolean | null;
  customerReturn?: boolean | null;
  username?: string | null;
  /** @format date-time */
  approvedDate?: string;
  /** @format date-time */
  productionEndDate?: string;
  /** @format date-time */
  estimateApprovalDate?: string;
  estimateApprovalUserName?: string | null;
  exported?: boolean | null;
}

export interface StyleAPIModelPaginationAPIModel {
  items?: StyleAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface SupplierAPIModel {
  /** @format int32 */
  supplierCode?: number;
  name?: string | null;
  telephoneNos?: string | null;
  mobileNos?: string | null;
  fax?: string | null;
  /** @format uuid */
  addressId?: string | null;
  addresses?: Address[] | null;
}

export interface SupplierAPIModelPaginationAPIModel {
  items?: SupplierAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface TokenAPIModel {
  token?: string | null;
  refreshToken?: string | null;
}

export interface UnauthorizedResult {
  /** @format int32 */
  statusCode?: number;
}

export interface UnitAPIModel {
  /** @format int32 */
  id?: number;
  code?: string | null;
  description?: string | null;
}

export interface UnitAPIModelPaginationAPIModel {
  items?: UnitAPIModel[] | null;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  currentPage?: number;
  /** @format int32 */
  totalItems?: number;
  sortColumn?: string | null;
  sortOrder?: string | null;
  filterColumn?: string | null;
  filterQuery?: string | null;
  /** @format int32 */
  totalPages?: number;
}

export interface UnprocessableEntityResult {
  /** @format int32 */
  statusCode?: number;
}

export interface UpdateAddressAPIModel {
  /** @format int32 */
  id?: number;
  /** @format uuid */
  addressId?: string;
  addressType?: AddressType;
  streetAddress?: string | null;
  city?: string | null;
  /** @format int32 */
  postCode?: number | null;
  state?: string | null;
  countryCode?: string | null;
  default?: boolean | null;
  /** @format int32 */
  buyerCode?: number | null;
}

export interface UpdateBankAPIModel {
  /** @format int32 */
  id?: number;
  name?: string | null;
  bankCode?: string | null;
  swiftCode?: string | null;
  telephoneNos?: string | null;
  currencyCode?: string | null;
  /** @format int32 */
  addressId?: number | null;
  /** @format double */
  loanLimit?: number;
}

export interface UpdateBuyerAPIModel {
  /** @format int32 */
  buyerCode?: number;
  status?: string | null;
  /** @format uuid */
  addressId?: string | null;
  name?: string | null;
  telephoneNos?: string | null;
  mobileNos?: string | null;
  fax?: string | null;
  cusdec?: string | null;
}

export interface UpdateCountryAPIModel {
  /** @format int32 */
  id?: number;
  code?: string | null;
  name?: string | null;
  /** @format byte */
  flag?: string | null;
}

export interface UpdateCurrencyAPIModel {
  /** @format int32 */
  id?: number;
  code?: string | null;
  name?: string | null;
  countryCode?: string | null;
  minor?: string | null;
}

export interface UpdateCurrencyExchangeAPIModel {
  /** @format int32 */
  id?: number;
  baseCurrency?: string | null;
  quoteCurrency?: string | null;
  /** @format double */
  rate?: number | null;
  /** @format date-time */
  exchangeDate?: string;
}

export interface UpdateFeatureAPIModel {
  /** @format int32 */
  id?: number;
  description?: string | null;
}

export interface UpdateGarmentTypeAPIModel {
  /** @format int32 */
  id?: number;
  typeName?: string | null;
}

export interface UpdatePortDestinationAPIModel {
  /** @format int32 */
  id?: number;
  countryCode?: string | null;
  destinationName?: string | null;
}

export interface UpdateStyleAPIModel {
  /** @format int32 */
  id?: number;
  /** @format int32 */
  buyerCode?: number;
  order?: string | null;
  /** @format date-time */
  orderDate?: string;
  /** @format int32 */
  typeCode?: number;
  styleCode?: string | null;
  unit?: string | null;
  /** @format double */
  quantity?: number | null;
  /** @format double */
  unitPrice?: number | null;
}

export interface UpdateSupplierAPIModel {
  /** @format int32 */
  supplierCode?: number;
  name?: string | null;
  telephoneNos?: string | null;
  mobileNos?: string | null;
  fax?: string | null;
  /** @format uuid */
  addressId?: string | null;
}

export interface UpdateUnitAPIModel {
  /** @format int32 */
  id?: number;
  code?: string | null;
  description?: string | null;
}

export interface UserAPIModel {
  id?: string | null;
  userName?: string | null;
  normalizedUserName?: string | null;
  email?: string | null;
  normalizedEmail?: string | null;
  emailConfirmed?: boolean;
  passwordHash?: string | null;
  securityStamp?: string | null;
  concurrencyStamp?: string | null;
  phoneNumber?: string | null;
  phoneNumberConfirmed?: boolean;
  twoFactorEnabled?: boolean;
  /** @format date-time */
  lockoutEnd?: string | null;
  lockoutEnabled?: boolean;
  /** @format int32 */
  accessFailedCount?: number;
  /** @format date-time */
  dateOfBirth?: string | null;
  knownAs?: string | null;
  /** @format date-time */
  created?: string;
  /** @format date-time */
  lastActive?: string | null;
  gender?: Gender;
  city?: string | null;
  country?: string | null;
  /** @format int32 */
  addressId?: number | null;
  /** @format byte */
  profilePhoto?: string | null;
  refreshToken?: string | null;
  /** @format date-time */
  refreshTokenExpiry?: string | null;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const data = !responseFormat
        ? r
        : await response[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title ApparelPro.WebApi
 * @version 1.0
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Address
     * @name AddressListList
     * @request GET:/api/address/list
     * @secure
     */
    addressListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<AddressAPIModelPaginationAPIModel, any>({
        path: `/api/address/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Address
     * @name GetAddressesByAddressIdAsync
     * @request GET:/api/address/list/AddressId/{addressId}
     * @secure
     */
    getAddressesByAddressIdAsync: (
      addressId: string,
      params: RequestParams = {},
    ) =>
      this.request<AddressAPIModel[], UnprocessableEntityResult>({
        path: `/api/address/list/AddressId/${addressId}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Address
     * @name GetAddressesForBuyerByAddressIdAsync
     * @request GET:/api/address/list/byAddressId
     * @secure
     */
    getAddressesForBuyerByAddressIdAsync: (
      query?: {
        /** @format uuid */
        addressId?: string;
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<
        AddressAPIModelPaginationAPIModel,
        UnprocessableEntityResult
      >({
        path: `/api/address/list/byAddressId`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Address
     * @name GetAddressByIdAndAddresIdAsync
     * @request GET:/api/address/list/byIdAndAddressId
     * @secure
     */
    getAddressByIdAndAddresIdAsync: (
      query?: {
        /** @format uuid */
        addressId?: string;
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<AddressAPIModel, UnprocessableEntityResult>({
        path: `/api/address/list/byIdAndAddressId`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Address
     * @name AddressCreate
     * @request POST:/api/address
     * @secure
     */
    addressCreate: (data: CreateAddressAPIModel, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/address`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Address
     * @name AddressUpdate
     * @request PUT:/api/address
     * @secure
     */
    addressUpdate: (
      data: UpdateAddressAPIModel,
      query?: {
        /** @format int32 */
        id?: number;
        /** @format uuid */
        addressId?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/address`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Address
     * @name AddressDelete
     * @request DELETE:/api/address/{id}/{addressId}
     * @secure
     */
    addressDelete: (
      id: number,
      addressId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/address/${id}/${addressId}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags ApparelProSeed
     * @name ApparelProSeedList
     * @request GET:/api/ApparelProSeed
     * @secure
     */
    apparelProSeedList: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/ApparelProSeed`,
        method: "GET",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Bank
     * @name BankListList
     * @request GET:/api/bank/list
     * @secure
     */
    bankListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<BankAPIModelPaginationAPIModel, any>({
        path: `/api/bank/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Bank
     * @name BankCreate
     * @request POST:/api/bank
     * @secure
     */
    bankCreate: (data: CreateBankAPIModel, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/bank`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Bank
     * @name BankUpdate
     * @request PUT:/api/bank
     * @secure
     */
    bankUpdate: (
      data: UpdateBankAPIModel,
      query?: {
        bankCode?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/bank`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Bank
     * @name GetBankByBankCodeAsync
     * @request GET:/api/bank/list/{code}
     * @secure
     */
    getBankByBankCodeAsync: (code: string, params: RequestParams = {}) =>
      this.request<BankAPIModel, UnprocessableEntityResult>({
        path: `/api/bank/list/${code}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Basis
     * @name BasisListList
     * @request GET:/api/basis/list
     * @secure
     */
    basisListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<BasisAPIModelPaginationAPIModel, any>({
        path: `/api/basis/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Buyers
     * @name BuyerListList
     * @request GET:/api/buyer/list
     * @secure
     */
    buyerListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<BuyerAPIModelPaginationAPIModel, any>({
        path: `/api/buyer/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Buyers
     * @name BuyerList1List
     * @request GET:/api/buyer/list-1
     * @secure
     */
    buyerList1List: (params: RequestParams = {}) =>
      this.request<BuyerAPIModel[], any>({
        path: `/api/buyer/list-1`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Buyers
     * @name BuyerCreate
     * @request POST:/api/buyer
     * @secure
     */
    buyerCreate: (data: CreateBuyerAPIModel, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/buyer`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Buyers
     * @name BuyerUpdate
     * @request PUT:/api/buyer
     * @secure
     */
    buyerUpdate: (
      data: UpdateBuyerAPIModel,
      query?: {
        /** @format int32 */
        buyerCode?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/buyer`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Buyers
     * @name BuyerPartialUpdate
     * @request PATCH:/api/buyer
     * @secure
     */
    buyerPartialUpdate: (params: RequestParams = {}) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/buyer`,
        method: "PATCH",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Buyers
     * @name GetBuyerByBuyerCodeAsync
     * @request GET:/api/buyer/list/{buyerCode}
     * @secure
     */
    getBuyerByBuyerCodeAsync: (buyerCode: number, params: RequestParams = {}) =>
      this.request<BuyerAPIModel, UnprocessableEntityResult>({
        path: `/api/buyer/list/${buyerCode}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Buyers
     * @name BuyerDelete
     * @request DELETE:/api/buyer/{buyerCode}
     * @secure
     */
    buyerDelete: (buyerCode: number, params: RequestParams = {}) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/buyer/${buyerCode}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Country
     * @name CountryListList
     * @request GET:/api/Country/list
     * @secure
     */
    countryListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<CountryAPIModelPaginationAPIModel, any>({
        path: `/api/Country/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Country
     * @name GetCountryByCodeAsync
     * @request GET:/api/Country/list/{code}
     * @secure
     */
    getCountryByCodeAsync: (code: string, params: RequestParams = {}) =>
      this.request<CountryAPIModel, UnprocessableEntityResult>({
        path: `/api/Country/list/${code}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Country
     * @name DoesCountryExistAsync
     * @request GET:/api/Country/list/does-Country-exist/{code}
     * @secure
     */
    doesCountryExistAsync: (code: string, params: RequestParams = {}) =>
      this.request<boolean, any>({
        path: `/api/Country/list/does-Country-exist/${code}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Country
     * @name GetCountryByPageNumberAsync
     * @request GET:/api/Country/list/paging/{pageSize}-{pageNumber}
     * @secure
     */
    getCountryByPageNumberAsync: (
      pageNumber: number,
      pageSize: number,
      params: RequestParams = {},
    ) =>
      this.request<CountryAPIModel[], any>({
        path: `/api/Country/list/paging/${pageSize}-${pageNumber}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Country
     * @name FilterCountriesByCodeAsync
     * @request GET:/api/Country/list/filter/{pageSize}-{pageNumber}
     * @secure
     */
    filterCountriesByCodeAsync: (
      pageNumber: number,
      pageSize: number,
      query?: {
        filter?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<CountryAPIModel[], any>({
        path: `/api/Country/list/filter/${pageSize}-${pageNumber}`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Country
     * @name CountryCreate
     * @request POST:/api/Country
     * @secure
     */
    countryCreate: (data: CreateCountryAPIModel, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Country`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Country
     * @name CountryDelete
     * @request DELETE:/api/Country
     * @secure
     */
    countryDelete: (
      query?: {
        code?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/Country`,
        method: "DELETE",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Country
     * @name CountryUpdate
     * @request PUT:/api/Country
     * @secure
     */
    countryUpdate: (
      data: UpdateCountryAPIModel,
      query?: {
        code?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/Country`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Country
     * @name CountryPartialUpdate
     * @request PATCH:/api/Country
     * @secure
     */
    countryPartialUpdate: (params: RequestParams = {}) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/Country`,
        method: "PATCH",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Currency
     * @name CurrencyListList
     * @request GET:/api/currency/list
     * @secure
     */
    currencyListList: (
      query?: {
        /** @format int32 */
        pageNumber?: number;
        /** @format int32 */
        pageSize?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<CurrencyAPIModelPaginationAPIModel, any>({
        path: `/api/currency/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Currency
     * @name GetCurrencyByCodeAsync
     * @request GET:/api/currency/list/{code}
     * @secure
     */
    getCurrencyByCodeAsync: (code: string, params: RequestParams = {}) =>
      this.request<CurrencyAPIModel, UnprocessableEntityResult>({
        path: `/api/currency/list/${code}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Currency
     * @name CurrencyDelete
     * @request DELETE:/api/currency/{code}
     * @secure
     */
    currencyDelete: (code: string, params: RequestParams = {}) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/currency/${code}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Currency
     * @name CurrencyCreate
     * @request POST:/api/currency
     * @secure
     */
    currencyCreate: (
      data: CreateCurrencyAPIModel,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/currency`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Currency
     * @name CurrencyUpdate
     * @request PUT:/api/currency
     * @secure
     */
    currencyUpdate: (
      data: UpdateCurrencyAPIModel,
      query?: {
        code?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/currency`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Currency
     * @name DoesCurrencyExistAsync
     * @request GET:/api/currency/list/does-currency-exist/{code}-{countryCode}
     * @secure
     */
    doesCurrencyExistAsync: (
      code: string,
      countryCode: string,
      params: RequestParams = {},
    ) =>
      this.request<boolean, any>({
        path: `/api/currency/list/does-currency-exist/${code}-${countryCode}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags CurrencyConversion
     * @name CurrencyConversionListList
     * @request GET:/api/currencyConversion/list
     * @secure
     */
    currencyConversionListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<CurrencyExchangeAPIModelPaginationAPIModel, any>({
        path: `/api/currencyConversion/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags CurrencyExchange
     * @name CurrencyExchangeListList
     * @request GET:/api/currencyExchange/list
     * @secure
     */
    currencyExchangeListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<CurrencyExchangeAPIModelPaginationAPIModel, any>({
        path: `/api/currencyExchange/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags CurrencyExchange
     * @name CurrencyExchangeListByDateList
     * @request GET:/api/currencyExchange/list/byDate
     * @secure
     */
    currencyExchangeListByDateList: (params: RequestParams = {}) =>
      this.request<CurrencyExchangeAPIModel[], any>({
        path: `/api/currencyExchange/list/byDate`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags CurrencyExchange
     * @name GetCurrencyExchangesByBaseCurrencyAsync
     * @request GET:/api/currencyExchange/list/{baseCurrency}
     * @secure
     */
    getCurrencyExchangesByBaseCurrencyAsync: (
      baseCurrency: string,
      params: RequestParams = {},
    ) =>
      this.request<CurrencyExchangeAPIModel[], UnprocessableEntityResult>({
        path: `/api/currencyExchange/list/${baseCurrency}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags CurrencyExchange
     * @name GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync
     * @request GET:/api/currencyExchange/list/{baseCurrency}/{quoteCurrency}/{date}
     * @secure
     */
    getCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync: (
      baseCurrency: string,
      quoteCurrency: string,
      date: string,
      params: RequestParams = {},
    ) =>
      this.request<CurrencyExchangeAPIModel, UnprocessableEntityResult>({
        path: `/api/currencyExchange/list/${baseCurrency}/${quoteCurrency}/${date}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags CurrencyExchange
     * @name CurrencyExchangeCreate
     * @request POST:/api/currencyExchange
     * @secure
     */
    currencyExchangeCreate: (
      data: CreateCurrencyExchangeAPIModel,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/currencyExchange`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags CurrencyExchange
     * @name CurrencyExchangeUpdate
     * @request PUT:/api/currencyExchange
     * @secure
     */
    currencyExchangeUpdate: (
      data: UpdateCurrencyExchangeAPIModel,
      query?: {
        baseCurrency?: string;
        quoteCurrency?: string;
        /** @format date-time */
        exchangeDate?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/currencyExchange`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags CurrencyExchange
     * @name CurrencyExchangeDelete
     * @request DELETE:/api/currencyExchange/{baseCurrency}/{quoteCurrency}/{exchangeDate}
     * @secure
     */
    currencyExchangeDelete: (
      baseCurrency: string,
      quoteCurrency: string,
      exchangeDate: string,
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/currencyExchange/${baseCurrency}/${quoteCurrency}/${exchangeDate}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Feature
     * @name FeatureListList
     * @request GET:/api/feature/list
     * @secure
     */
    featureListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<BuyerAPIModelPaginationAPIModel, any>({
        path: `/api/feature/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Feature
     * @name GetFeatureByFeatureIdAsync
     * @request GET:/api/feature/list/{id}
     * @secure
     */
    getFeatureByFeatureIdAsync: (id: number, params: RequestParams = {}) =>
      this.request<FeatureAPIModel, UnprocessableEntityResult>({
        path: `/api/feature/list/${id}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Feature
     * @name FeatureCreate
     * @request POST:/api/feature
     * @secure
     */
    featureCreate: (data: CreateFeatureAPIModel, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/feature`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Feature
     * @name FeatureUpdate
     * @request PUT:/api/feature
     * @secure
     */
    featureUpdate: (
      data: UpdateFeatureAPIModel,
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/feature`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Feature
     * @name FeatureDelete
     * @request DELETE:/api/feature/{id}
     * @secure
     */
    featureDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/feature/${id}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags GarmentType
     * @name GarmentTypeListList
     * @request GET:/api/garmentType/list
     * @secure
     */
    garmentTypeListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<GarmentTypeAPIModelPaginationAPIModel, any>({
        path: `/api/garmentType/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags GarmentType
     * @name GarmentTypeUpdate
     * @request PUT:/api/garmentType
     * @secure
     */
    garmentTypeUpdate: (
      data: UpdateGarmentTypeAPIModel,
      query?: {
        /** @format int32 */
        id?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/garmentType`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags GarmentType
     * @name GarmentTypePartialUpdate
     * @request PATCH:/api/garmentType
     * @secure
     */
    garmentTypePartialUpdate: (params: RequestParams = {}) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/garmentType`,
        method: "PATCH",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags PO
     * @name PoListList
     * @request GET:/api/po/list
     * @secure
     */
    poListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<POAPIModelPaginationAPIModel, any>({
        path: `/api/po/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags PO
     * @name GetPurchaseOrderByBuyerAndOrderAsync
     * @request GET:/api/po/list/buyer/order
     * @secure
     */
    getPurchaseOrderByBuyerAndOrderAsync: (
      query?: {
        /** @format int32 */
        buyer?: number;
        order?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<POAPIModel, UnprocessableEntityResult>({
        path: `/api/po/list/buyer/order`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags PortDestination
     * @name PortDestinationListList
     * @request GET:/api/portDestination/list
     * @secure
     */
    portDestinationListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<PortDestinationAPIModelPaginationAPIModel, any>({
        path: `/api/portDestination/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags PortDestination
     * @name PortDestinationCreate
     * @request POST:/api/portDestination
     * @secure
     */
    portDestinationCreate: (
      data: CreatePortDestinationAPIModel,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/portDestination`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags PortDestination
     * @name PortDestinationUpdate
     * @request PUT:/api/portDestination
     * @secure
     */
    portDestinationUpdate: (
      data: UpdatePortDestinationAPIModel,
      query?: {
        /** @format int32 */
        id?: number;
        countryCode?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/portDestination`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags PortDestination
     * @name GetPortDestinationByIdAndCountryCodeAsync
     * @request GET:/api/portDestination/list/{id}/{countryCode}
     * @secure
     */
    getPortDestinationByIdAndCountryCodeAsync: (
      id: number,
      countryCode: string,
      params: RequestParams = {},
    ) =>
      this.request<PortDestinationAPIModel, UnprocessableEntityResult>({
        path: `/api/portDestination/list/${id}/${countryCode}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags PortDestination
     * @name PortDestinationDelete
     * @request DELETE:/api/portDestination/{id}/{countryCode}
     * @secure
     */
    portDestinationDelete: (
      id: number,
      countryCode: string,
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/portDestination/${id}/${countryCode}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Security
     * @name SecurityRefreshTokenCreate
     * @request POST:/api/security/refresh-token
     * @secure
     */
    securityRefreshTokenCreate: (
      data: TokenAPIModel,
      params: RequestParams = {},
    ) =>
      this.request<BadRequestResult, any>({
        path: `/api/security/refresh-token`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Security
     * @name SecurityRevokeCreate
     * @request POST:/api/security/revoke
     * @secure
     */
    securityRevokeCreate: (
      query?: {
        user?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<NoContentResult, BadRequestResult>({
        path: `/api/security/revoke`,
        method: "POST",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StyleDetails
     * @name StyleDetailsListList
     * @request GET:/api/styleDetails/list
     * @secure
     */
    styleDetailsListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<StyleAPIModelPaginationAPIModel, any>({
        path: `/api/styleDetails/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StyleDetails
     * @name StyleDetailsCreate
     * @request POST:/api/styleDetails
     * @secure
     */
    styleDetailsCreate: (
      data: CreateStyleDetailsAPIModel,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/styleDetails`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags StyleDetails
     * @name StyleDetailsPartialUpdate
     * @request PATCH:/api/styleDetails
     * @secure
     */
    styleDetailsPartialUpdate: (params: RequestParams = {}) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/styleDetails`,
        method: "PATCH",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags StyleDetails
     * @name StyleDetailsUpdate
     * @request PUT:/api/styleDetails
     * @secure
     */
    styleDetailsUpdate: (
      data: UpdateStyleAPIModel,
      query?: {
        /** @format int32 */
        buyerCode?: number;
        order?: string;
        /** @format int32 */
        typeCode?: number;
        style?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/styleDetails`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags StyleDetails
     * @name StyleDetailsDelete
     * @request DELETE:/api/styleDetails
     * @secure
     */
    styleDetailsDelete: (
      query?: {
        /** @format int32 */
        buyer?: number;
        order?: string;
        /** @format int32 */
        type?: number;
        _style?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/styleDetails`,
        method: "DELETE",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags StyleDetails
     * @name GetStyleDetailsByBuyerAndOrderAsync
     * @request GET:/api/styleDetails/list/buyer/order
     * @secure
     */
    getStyleDetailsByBuyerAndOrderAsync: (
      query?: {
        /** @format int32 */
        buyerCode?: number;
        order?: string;
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<StyleAPIModelPaginationAPIModel, UnprocessableEntityResult>({
        path: `/api/styleDetails/list/buyer/order`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags StyleDetails
     * @name GetStyleDetailsByBuyerOrderTypeStyleAsync
     * @request GET:/api/styleDetails/list/{buyer}/{order}/{type}/{style}
     * @secure
     */
    getStyleDetailsByBuyerOrderTypeStyleAsync: (
      buyer: number,
      order: string,
      type: number,
      style: string,
      params: RequestParams = {},
    ) =>
      this.request<CountryAPIModel, UnprocessableEntityResult>({
        path: `/api/styleDetails/list/${buyer}/${order}/${type}/${style}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Supplier
     * @name SupplierListList
     * @request GET:/api/supplier/list
     * @secure
     */
    supplierListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<SupplierAPIModelPaginationAPIModel, any>({
        path: `/api/supplier/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Supplier
     * @name GetSupplierBySupplierCodeAsync
     * @request GET:/api/supplier/list/{SupplierCode}
     * @secure
     */
    getSupplierBySupplierCodeAsync: (
      supplierCode: number,
      params: RequestParams = {},
    ) =>
      this.request<CountryAPIModel, UnprocessableEntityResult>({
        path: `/api/supplier/list/${supplierCode}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Supplier
     * @name SupplierCreate
     * @request POST:/api/supplier
     * @secure
     */
    supplierCreate: (
      data: CreateSupplierAPIModel,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/supplier`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Supplier
     * @name SupplierUpdate
     * @request PUT:/api/supplier
     * @secure
     */
    supplierUpdate: (
      data: UpdateSupplierAPIModel,
      query?: {
        /** @format int32 */
        supplierCode?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/supplier`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Supplier
     * @name SupplierDelete
     * @request DELETE:/api/supplier/{code}
     * @secure
     */
    supplierDelete: (
      code: string,
      query?: {
        /** @format int32 */
        supplierCode?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/supplier/${code}`,
        method: "DELETE",
        query: query,
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Unit
     * @name UnitListList
     * @request GET:/api/unit/list
     * @secure
     */
    unitListList: (
      query?: {
        /** @format int32 */
        pageSize?: number;
        /** @format int32 */
        pageNumber?: number;
        sortColumn?: string;
        sortOrder?: string;
        filterColumn?: string;
        filterQuery?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<UnitAPIModelPaginationAPIModel, any>({
        path: `/api/unit/list`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Unit
     * @name UnitCreate
     * @request POST:/api/unit
     * @secure
     */
    unitCreate: (data: CreateUnitAPIModel, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/unit`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Unit
     * @name UnitUpdate
     * @request PUT:/api/unit
     * @secure
     */
    unitUpdate: (
      data: UpdateUnitAPIModel,
      query?: {
        code?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, UnprocessableEntityResult>({
        path: `/api/unit`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Unit
     * @name GetUnitByCodeAsync
     * @request GET:/api/unit/list/{code}
     * @secure
     */
    getUnitByCodeAsync: (code: string, params: RequestParams = {}) =>
      this.request<UnitAPIModel, any>({
        path: `/api/unit/list/${code}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Unit
     * @name DoesUnitExistAsync
     * @request GET:/api/unit/list/does-unit-exist/{code}
     * @secure
     */
    doesUnitExistAsync: (code: string, params: RequestParams = {}) =>
      this.request<boolean, any>({
        path: `/api/unit/list/does-unit-exist/${code}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserListList
     * @request GET:/api/user/list
     * @secure
     */
    userListList: (params: RequestParams = {}) =>
      this.request<UserAPIModel[], any>({
        path: `/api/user/list`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name GetUserByEmailAsync
     * @request GET:/api/user/list/{email}
     * @secure
     */
    getUserByEmailAsync: (
      email: string,
      query?: {
        email?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<UserAPIModel, UnprocessableEntityResult>({
        path: `/api/user/list/${email}`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserRegisterCreate
     * @request POST:/api/user/register
     * @secure
     */
    userRegisterCreate: (
      data: RegisterUserAPIModel,
      params: RequestParams = {},
    ) =>
      this.request<RegisteredUserAPIModel, BadRequestResult>({
        path: `/api/user/register`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserCreate
     * @request POST:/api/user
     * @secure
     */
    userCreate: (data: UserAPIModel, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/user`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserLoginCreate
     * @request POST:/api/user/login
     * @secure
     */
    userLoginCreate: (data: LoginUserAPIModel, params: RequestParams = {}) =>
      this.request<RegisterUserAPIModel, BadRequestResult | UnauthorizedResult>(
        {
          path: `/api/user/login`,
          method: "POST",
          body: data,
          secure: true,
          type: ContentType.Json,
          format: "json",
          ...params,
        },
      ),

    /**
     * No description
     *
     * @tags User
     * @name UserRefreshTokenCreate
     * @request POST:/api/user/refresh-token
     * @secure
     */
    userRefreshTokenCreate: (data: any, params: RequestParams = {}) =>
      this.request<string, any>({
        path: `/api/user/refresh-token`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserRefreshATokenCreate
     * @request POST:/api/user/refresh-a-token
     * @secure
     */
    userRefreshATokenCreate: (
      data: RegisteredUserAPIModel,
      params: RequestParams = {},
    ) =>
      this.request<RegisterUserAPIModel, any>({
        path: `/api/user/refresh-a-token`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
}
