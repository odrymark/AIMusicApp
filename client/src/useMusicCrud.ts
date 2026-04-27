import { Api } from "./Api.ts";
import type { UserLoginReqDto, UserCreateReqDto } from "./Api.ts";
import { userAtom } from "./atoms/userAtom.ts";
import { useAtom } from "jotai";

const api = new Api({
    baseUrl: "http://167.86.77.173:8080",
    baseApiParams: {
        credentials: "include"
    }
});

export type UserInfo = {
    id: string;
    username: string;
    isAdmin: boolean;
};

export type Song = {
    id: string;
    title: string;
    songKey: string;
    url: string;
    artist: string;
    image: string | null;
    isPublic: boolean;
}

export type Playlist = {
    id: string;
    title: string;
    creatorUser: string;
    image: string | null;
    isPublic: boolean;
    songs: Song[];
}

let refreshPromise: Promise<boolean> | null = null;

export default function useMusicCrud() {
    const [, setUser] = useAtom(userAtom);

    async function refreshToken(): Promise<boolean> {
        if (refreshPromise) return refreshPromise;

        refreshPromise = api.api.authRefresh()
            .then(res => res.ok)
            .catch(() => false)
            .finally(() => { refreshPromise = null; });

        return refreshPromise;
    }

    async function withAuthRetry<T>(fn: () => Promise<Response>, transform: (res: Response) => Promise<T>): Promise<T> {
        let response: Response;

        try {
            response = await fn();
        } catch (err: any) {
            if (err?.status === 401 || (err instanceof Response && err.status === 401)) {
                const refreshed = await refreshToken();
                if (!refreshed) {
                    setUser(null);
                    throw new Error("Session expired. Please log in again.");
                }
                response = await fn();
            } else {
                throw err;
            }
        }

        return transform(response!);
    }

    async function login(dto: UserLoginReqDto): Promise<void> {
        try {
            const response = await api.api.authLogin(dto);
            const userData = await response.json() as unknown as UserInfo;
            setUser(userData);
        } catch (error) {
            console.error("Login failed:", error);
            throw error;
        }
    }

    async function logout(): Promise<void> {
        try {
            await api.api.authLogout();
            setUser(null);
        } catch (error) {
            console.error("Logout failed:", error);
            throw error;
        }
    }

    async function getMe(): Promise<void> {
        try {
            const response = await api.api.authGetMe();
            setUser(await response.json() as UserInfo);
        } catch (err: any) {
            if (err?.status === 401 || (err instanceof Response && err.status === 401)) {
                const refreshed = await refreshToken();
                if (!refreshed) { setUser(null); return; }

                try {
                    const response = await api.api.authGetMe();
                    setUser(await response.json() as UserInfo);
                } catch {
                    setUser(null);
                }
            } else {
                setUser(null);
            }
        }
    }

    async function createUser(dto: UserCreateReqDto): Promise<void> {
        try {
            await api.api.userCreateUser(dto);
            await login({ username: dto.username, password: dto.password });
        } catch (error) {
            console.error("Creating user failed:", error);
            throw error;
        }
    }

    async function uploadSong(file: File, title: string, artist: string, isPublic: boolean, image?: File): Promise<void> {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("title", title);
        formData.append("artist", artist);
        formData.append("isPublic", String(isPublic));
        if (image) formData.append("image", image);

        await withAuthRetry(
            () => api.api.songUploadSong(formData as any),
            () => Promise.resolve(undefined)
        );
    }

    async function getUserSongs(): Promise<Song[]> {
        return withAuthRetry(
            () => api.api.songGetUserSongs(),
            (res) => res.json() as Promise<Song[]>
        );
    }

    async function getSongs(): Promise<Song[]> {
        return withAuthRetry(
            () => api.api.songGetSongs(),
            (res) => res.json() as Promise<Song[]>
        );
    }

    async function getSignedUrl(key: string): Promise<string> {
        return withAuthRetry(
            () => api.api.songGetSignedUrl({ key }),
            (res) => res.text()
        );
    }

    async function editSong(id: string, title: string, artist: string, isPublic: boolean, prevImgKey?: string, image?: File): Promise<void> {
        const formData = new FormData();
        formData.append("id", id);
        formData.append("title", title);
        formData.append("artist", artist);
        formData.append("isPublic", String(isPublic));
        if (prevImgKey) formData.append("prevImgKey", prevImgKey);
        if (image) formData.append("image", image);

        await withAuthRetry(
            () => api.api.songEditSong(formData as any),
            () => Promise.resolve(undefined)
        );
    }

    async function createPlaylist(title: string, songIds: string[], isPublic: boolean, image?: File): Promise<void> {
        const formData = new FormData();
        formData.append("title", title);
        formData.append("isPublic", String(isPublic));
        songIds.forEach(id => formData.append("songIds", id));
        if (image) formData.append("image", image);

        await withAuthRetry(
            () => api.api.playlistCreatePlaylist(formData as any),
            () => Promise.resolve(undefined)
        );
    }

    async function editPlaylist(id: string, title: string, songIds: string[], isPublic: boolean, prevImgKey?: string, image?: File): Promise<void> {
        const formData = new FormData();
        formData.append("id", id);
        formData.append("title", title);
        formData.append("isPublic", String(isPublic));
        songIds.forEach(songId => formData.append("songIds", songId));
        if (prevImgKey) formData.append("prevImgKey", prevImgKey);
        if (image) formData.append("image", image);

        await withAuthRetry(
            () => api.api.playlistEditPlaylist(formData as any),
            () => Promise.resolve(undefined)
        );
    }

    async function getPlaylists(): Promise<Playlist[]> {
        return withAuthRetry(
            () => api.api.playlistGetPlaylists(),
            (res) => res.json() as Promise<Playlist[]>
        );
    }

    async function getUserPlaylists(): Promise<Playlist[]> {
        return withAuthRetry(
            () => api.api.playlistGetUserPlaylists(),
            (res) => res.json() as Promise<Playlist[]>
        );
    }

    async function getFunctionState(featureKey: string): Promise<boolean> {
        const res = await api.api.fhGetFeatureStatus(featureKey);
        return await res.json() as boolean;
    }

    return {
        login,
        logout,
        getMe,
        createUser,
        uploadSong,
        getUserSongs,
        getSongs,
        getSignedUrl,
        editSong,
        createPlaylist,
        getPlaylists,
        getUserPlaylists,
        editPlaylist,
        getFunctionState
    };
}
