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

export interface UserLoginReqDto {
  /**
   * @minLength 3
   * @maxLength 50
   * @pattern ^[a-zA-Z0-9_]+$
   */
  username: string;
  /** @minLength 8 */
  password: string;
}

export interface UserCreateReqDto {
  /**
   * @minLength 3
   * @maxLength 50
   * @pattern ^[a-zA-Z0-9_]+$
   */
  username: string;
  /**
   * @format email
   * @minLength 1
   */
  email: string;
  /** @minLength 8 */
  password: string;
  /** @minLength 1 */
  passwordConfirm: string;
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
  public baseUrl: string = "http://0.0.0.0:8080";
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

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
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
 * @title My Title
 * @version 1.0.0
 * @baseUrl http://0.0.0.0:8080
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Auth
     * @name AuthLogin
     * @request POST:/api/auth/login
     */
    authLogin: (data: UserLoginReqDto, params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/auth/login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthLogout
     * @request POST:/api/auth/logout
     */
    authLogout: (params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/auth/logout`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthGetMe
     * @request GET:/api/auth/me
     */
    authGetMe: (params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/auth/me`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthRefresh
     * @request POST:/api/auth/refresh
     */
    authRefresh: (params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/auth/refresh`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Fh
     * @name FhGetFeatureStatus
     * @request POST:/api/featurehub/getFeatureState
     */
    fhGetFeatureStatus: (data: string, params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/featurehub/getFeatureState`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Playlist
     * @name PlaylistGetPlaylists
     * @request GET:/api/playlist/getPlaylists
     */
    playlistGetPlaylists: (params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/playlist/getPlaylists`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Playlist
     * @name PlaylistGetUserPlaylists
     * @request GET:/api/playlist/getUserPlaylists
     */
    playlistGetUserPlaylists: (params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/playlist/getUserPlaylists`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Playlist
     * @name PlaylistCreatePlaylist
     * @request POST:/api/playlist/createPlaylist
     */
    playlistCreatePlaylist: (
      data: {
        title?: string | null;
        /**
         * @maxItems 100
         * @minItems 2
         */
        songIds?: string[] | null;
        /** @format binary */
        image?: File | null;
        isPublic?: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<Blob, any>({
        path: `/api/playlist/createPlaylist`,
        method: "POST",
        body: data,
        type: ContentType.FormData,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Playlist
     * @name PlaylistEditPlaylist
     * @request POST:/api/playlist/editPlaylist
     */
    playlistEditPlaylist: (
      data: {
        /** @format guid */
        id?: string;
        title?: string | null;
        /**
         * @maxItems 100
         * @minItems 2
         */
        songIds?: string[] | null;
        isPublic?: boolean;
        /** @format binary */
        image?: File | null;
        prevImgKey?: string | null;
      },
      params: RequestParams = {},
    ) =>
      this.request<Blob, any>({
        path: `/api/playlist/editPlaylist`,
        method: "POST",
        body: data,
        type: ContentType.FormData,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Song
     * @name SongUploadSong
     * @request POST:/api/song/uploadSong
     */
    songUploadSong: (
      data: {
        /** @format binary */
        file?: File | null;
        title?: string | null;
        artist?: string | null;
        isPublic?: boolean;
        /** @format binary */
        image?: File | null;
      },
      params: RequestParams = {},
    ) =>
      this.request<Blob, any>({
        path: `/api/song/uploadSong`,
        method: "POST",
        body: data,
        type: ContentType.FormData,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Song
     * @name SongGetUserSongs
     * @request GET:/api/song/getUserSongs
     */
    songGetUserSongs: (params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/song/getUserSongs`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Song
     * @name SongGetSongs
     * @request GET:/api/song/getSongs
     */
    songGetSongs: (params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/song/getSongs`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Song
     * @name SongGetSignedUrl
     * @request GET:/api/song/getSignedUrl
     */
    songGetSignedUrl: (
      query?: {
        key?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<Blob, any>({
        path: `/api/song/getSignedUrl`,
        method: "GET",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Song
     * @name SongEditSong
     * @request POST:/api/song/editSong
     */
    songEditSong: (
      data: {
        /** @format guid */
        id?: string;
        title?: string | null;
        artist?: string | null;
        isPublic?: boolean;
        /** @format binary */
        image?: File | null;
        prevImgKey?: string | null;
      },
      params: RequestParams = {},
    ) =>
      this.request<Blob, any>({
        path: `/api/song/editSong`,
        method: "POST",
        body: data,
        type: ContentType.FormData,
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserCreateUser
     * @request POST:/api/user/createUser
     */
    userCreateUser: (data: UserCreateReqDto, params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/user/createUser`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),
  };
}
