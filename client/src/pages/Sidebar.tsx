import { useAtomValue, useSetAtom } from "jotai";
import { useNavigate, Outlet, useLocation } from "react-router-dom";
import { userAtom } from "../atoms/userAtom";
import { currentSongAtom } from "../atoms/currentSongAtom";
import { currentPlaylistAtom } from "../atoms/currentPlaylistAtom";
import useMusicCrud from "../useMusicCrud";
import { useEffect, useRef, useState } from "react";

function navButtonClass(pathname: string, targetPath: string): string {
    const isActive = pathname === targetPath;
    return `flex items-center gap-3 p-3 rounded-lg transition-colors ${
        isActive ? "bg-primary/10 text-primary font-medium" : "hover:bg-base-200"
    }`;
}

export default function Sidebar() {
    const { getMe, logout, getSignedUrl } = useMusicCrud();
    const user = useAtomValue(userAtom);
    const navigate = useNavigate();
    const location = useLocation();

    const currentSong = useAtomValue(currentSongAtom);
    const setCurrentSong = useSetAtom(currentSongAtom);
    const currentPlaylist = useAtomValue(currentPlaylistAtom);
    const setCurrentPlaylist = useSetAtom(currentPlaylistAtom);

    const audioRef = useRef<HTMLAudioElement | null>(null);
    const [isPlaying, setIsPlaying] = useState(false);
    const [progress, setProgress] = useState(0);
    const [duration, setDuration] = useState(0);
    const [volume, setVolume] = useState(1);
    const [resolvedUrl, setResolvedUrl] = useState<string | null>(null);

    useEffect(() => {
        getMe().catch(() => {});
    }, []);

    useEffect(() => {
        if (!currentSong) return;

        let cancelled = false;

        getSignedUrl(currentSong.songKey)
            .then(url => { if (!cancelled) setResolvedUrl(url); })
            .catch(() => { if (!cancelled) setResolvedUrl(null); });

        return () => { cancelled = true; };
    }, [currentSong]);

    useEffect(() => {
        if (!resolvedUrl) return;

        if (audioRef.current) {
            audioRef.current.pause();
        }

        const audio = new Audio(resolvedUrl);
        audioRef.current = audio;
        audio.volume = volume;

        audio.addEventListener("timeupdate", () => {
            setProgress(audio.currentTime);
        });
        audio.addEventListener("loadedmetadata", () => {
            setDuration(audio.duration);
        });
        audio.addEventListener("ended", () => {
            setIsPlaying(false);
            setProgress(0);

            if (currentPlaylist && currentSong) {
                const idx = currentPlaylist.songs.findIndex(s => s.id === currentSong.id);
                const nextSong = currentPlaylist.songs[idx + 1];
                if (nextSong) {
                    setCurrentSong(nextSong);
                }
            }
        });

        audio.play().then(() => setIsPlaying(true)).catch(() => setIsPlaying(false));

        return () => {
            audio.pause();
            audio.src = "";
        };
    }, [resolvedUrl]);

    const togglePlay = () => {
        if (!audioRef.current) return;
        if (isPlaying) {
            audioRef.current.pause();
            setIsPlaying(false);
        } else {
            audioRef.current.play();
            setIsPlaying(true);
        }
    };

    const handleSeek = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = Number(e.target.value);
        if (audioRef.current) audioRef.current.currentTime = val;
        setProgress(val);
    };

    const handleVolume = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = Number(e.target.value);
        setVolume(val);
        if (audioRef.current) audioRef.current.volume = val;
    };

    const formatTime = (s: number) => {
        if (isNaN(s)) return "0:00";
        const m = Math.floor(s / 60);
        const sec = Math.floor(s % 60);
        return `${m}:${sec.toString().padStart(2, "0")}`;
    };

    const currentSongIndex =
        currentPlaylist && currentSong
            ? currentPlaylist.songs.findIndex(s => s.id === currentSong.id)
            : -1;

    const hasPrev = currentSongIndex > 0;
    const hasNext =
        currentPlaylist != null &&
        currentSongIndex >= 0 &&
        currentSongIndex < currentPlaylist.songs.length - 1;

    const handlePrev = () => {
        if (!currentPlaylist || currentSongIndex <= 0) return;
        setCurrentSong(currentPlaylist.songs[currentSongIndex - 1]);
        setProgress(0);
    };

    const handleNext = () => {
        if (!currentPlaylist || currentSongIndex < 0 || currentSongIndex >= currentPlaylist.songs.length - 1) return;
        setCurrentSong(currentPlaylist.songs[currentSongIndex + 1]);
        setProgress(0);
    };

    const username = user?.username ?? "Guest";

    return (
        <div className="flex h-screen overflow-hidden bg-base-200">
            <aside className="bg-base-100 border-r border-base-300 w-72 flex-shrink-0 flex flex-col h-full">
                <div className="flex items-center gap-3 p-4">
                    <div className="w-10 h-10 rounded-full overflow-hidden border-2 border-primary">
                        <img
                            src={`https://api.dicebear.com/7.x/avataaars/svg?seed=${username}`}
                            alt="User avatar"
                            className="w-full h-full"
                        />
                    </div>
                    <span className="font-medium text-base-content">{username}</span>
                </div>

                {!user && (
                    <button
                        className="btn btn-primary btn-sm mt-2 ml-10 w-50"
                        onClick={() => navigate("/login")}
                    >
                        Login
                    </button>
                )}

                <div className="flex flex-col justify-between flex-1 p-6">
                    <nav className="flex flex-col justify-center space-y-2 flex-1">
                        <button
                            className={navButtonClass(location.pathname, "/home")}
                            onClick={() => navigate("/home")}
                        >
                            <span>Home</span>
                        </button>
                        {user && (
                            <>
                                <button
                                    className={navButtonClass(location.pathname, "/myMusic")}
                                    onClick={() => navigate("/myMusic")}
                                >
                                    <span>My Music</span>
                                </button>

                                <button
                                    className={navButtonClass(location.pathname, "/uploadSong")}
                                    onClick={() => navigate("/uploadSong")}
                                >
                                    <span>Upload Song</span>
                                </button>

                                <button
                                    className={navButtonClass(location.pathname, "/createPlaylist")}
                                    onClick={() => navigate("/createPlaylist")}
                                >
                                    <span>Create Playlist</span>
                                </button>
                            </>
                        )}
                    </nav>

                    {user && (
                        <div className="flex justify-center mt-4">
                            <button className="btn btn-primary w-60" onClick={() => {logout(); navigate("/home")}}>
                                Logout
                            </button>
                        </div>
                    )}
                </div>
            </aside>

            <main className="flex-1 flex flex-col overflow-hidden">
                <div className="flex-1 overflow-hidden p-6 md:p-10">
                    <Outlet />
                </div>
            </main>

            {currentSong && (
                <div className="fixed bottom-6 right-6 z-50 w-80 rounded-2xl shadow-2xl border border-base-300 bg-base-100 p-4 flex flex-col gap-3">

                    {/* Playlist info */}
                    {currentPlaylist && (
                        <div className="flex items-center gap-2 pb-2 border-b border-base-300">
                            {currentPlaylist.image ? (
                                <img
                                    src={currentPlaylist.image}
                                    alt={currentPlaylist.title}
                                    className="w-8 h-8 rounded-md object-cover flex-shrink-0"
                                />
                            ) : (
                                <div className="w-8 h-8 rounded-md bg-base-300 flex items-center justify-center flex-shrink-0 text-base">
                                    🎶
                                </div>
                            )}
                            <div className="min-w-0">
                                <div className="text-xs text-base-content/40 uppercase tracking-wide">Playing from</div>
                                <div className="text-xs font-semibold truncate">{currentPlaylist.title}</div>
                            </div>
                        </div>
                    )}

                    {/* Song info */}
                    <div className="flex items-center gap-3">
                        {currentSong.image ? (
                            <img
                                src={currentSong.image}
                                alt={currentSong.title}
                                className="w-10 h-10 rounded-md object-cover flex-shrink-0"
                            />
                        ) : (
                            <div className="w-10 h-10 rounded-md bg-base-300 flex items-center justify-center flex-shrink-0 text-base-content/40 text-xs">
                                🎵
                            </div>
                        )}
                        <div className="flex-1 min-w-0">
                            <div className="font-semibold text-sm truncate">{currentSong.title}</div>
                            <div className="text-xs text-base-content/50 truncate">
                                {!resolvedUrl ? "Loading..." : (currentSong.artist ?? "Unknown artist")}
                            </div>
                        </div>
                        <button
                            onClick={() => {
                                audioRef.current?.pause();
                                setCurrentSong(null);
                                setCurrentPlaylist(null);
                                setIsPlaying(false);
                                setProgress(0);
                            }}
                            className="btn btn-ghost btn-xs text-base-content/40 hover:text-base-content"
                        >
                            ✕
                        </button>
                    </div>

                    {/* Progress bar */}
                    <div className="flex items-center gap-2 text-xs text-base-content/50">
                        <span className="w-8 text-right">{formatTime(progress)}</span>
                        <input
                            type="range"
                            min={0}
                            max={duration || 0}
                            value={progress}
                            onChange={handleSeek}
                            className="range range-xs range-primary flex-1"
                            disabled={!resolvedUrl}
                        />
                        <span className="w-8">{formatTime(duration)}</span>
                    </div>

                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-1 text-base-content/50">
                            <span className="text-xs">🔈</span>
                            <input
                                type="range"
                                min={0}
                                max={1}
                                step={0.01}
                                value={volume}
                                onChange={handleVolume}
                                className="range range-xs w-20"
                            />
                        </div>

                        {/* Playback controls: prev, play/pause, next */}
                        <div className="flex items-center gap-1">
                            {/* Previous button */}
                            <button
                                onClick={handlePrev}
                                className="btn btn-ghost btn-sm btn-circle"
                                disabled={!hasPrev || !resolvedUrl}
                                title="Previous song"
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="w-4 h-4">
                                    <path d="M6 6h2v12H6zm3.5 6 8.5 6V6z" />
                                </svg>
                            </button>

                            {/* Play / Pause button */}
                            <button
                                onClick={togglePlay}
                                className="btn btn-primary btn-sm btn-circle"
                                disabled={!resolvedUrl}
                            >
                                {!resolvedUrl ? (
                                    <span className="loading loading-spinner loading-xs" />
                                ) : isPlaying ? (
                                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" className="w-4 h-4">
                                        <path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z" />
                                    </svg>
                                ) : (
                                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" className="w-4 h-4 ml-0.5">
                                        <path d="M8 5v14l11-7z" />
                                    </svg>
                                )}
                            </button>

                            {/* Next button */}
                            <button
                                onClick={handleNext}
                                className="btn btn-ghost btn-sm btn-circle"
                                disabled={!hasNext || !resolvedUrl}
                                title="Next song"
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="w-4 h-4">
                                    <path d="M6 18l8.5-6L6 6v12zm8.5-6v6h2V6h-2v6z" />
                                </svg>
                            </button>
                        </div>

                        <div className="w-20" />
                    </div>
                </div>
            )}
        </div>
    );
}