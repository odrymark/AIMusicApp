import { useState, useEffect } from "react";
import { useAtomValue, useSetAtom } from "jotai";
import { useNavigate } from "react-router-dom";
import { userAtom } from "../atoms/userAtom";
import { currentSongAtom } from "../atoms/currentSongAtom";
import { currentPlaylistAtom } from "../atoms/currentPlaylistAtom";
import useMusicCrud, { type Song, type Playlist } from "../useMusicCrud.ts";

export default function MyMusic() {
    const user = useAtomValue(userAtom);
    const { getUserSongs, getUserPlaylists, getSignedUrl, getFunctionState } = useMusicCrud();
    const setCurrentSong = useSetAtom(currentSongAtom);
    const setCurrentPlaylist = useSetAtom(currentPlaylistAtom);
    const navigate = useNavigate();

    const editSongFeatureKey = "edit_song";
    const editPlaylistFeatureKey = "edit_playlist";
    const [isSongEditOn, setIsSongEditOn] = useState(false);
    const [isPlaylistEditOn, setIsPlaylistEditOn] = useState(false);

    const [songs, setSongs] = useState<Song[]>([]);
    const [playlists, setPlaylists] = useState<Playlist[]>([]);
    const [search, setSearch] = useState("");
    const [activeTab, setActiveTab] = useState<"songs" | "playlists">("songs");

    useEffect(() => {
        const loadData = async () => {
            const songsRes = await getUserSongs();
            const songsWithUrls = await Promise.all(
                songsRes.map(async (song) => ({
                    ...song,
                    songUrl: await getSignedUrl(song.songKey),
                    image: song.image ? await getSignedUrl(song.image) : null,
                }))
            );
            setSongs(songsWithUrls);

            const playlistsRes = await getUserPlaylists();
            const playlistsWithUrls = await Promise.all(
                playlistsRes.map(async (playlist) => ({
                    ...playlist,
                    image: playlist.image ? await getSignedUrl(playlist.image) : null,
                    songs: await Promise.all(
                        playlist.songs.map(async (song) => ({
                            ...song,
                            songUrl: await getSignedUrl(song.songKey),
                            image: song.image ? await getSignedUrl(song.image) : null,
                        }))
                    ),
                }))
            );
            setPlaylists(playlistsWithUrls);

            const isSongEdit = await getFunctionState(editSongFeatureKey);
            const isPlaylistEdit = await getFunctionState(editPlaylistFeatureKey);
            setIsSongEditOn(isSongEdit);
            setIsPlaylistEditOn(isPlaylistEdit);
        };

        loadData();
    }, [user]);

    const handlePlayPlaylist = async (playlist: Playlist) => {
        if (playlist.songs.length === 0) return;
        setCurrentPlaylist(playlist);
        setCurrentSong(playlist.songs[0]);
    };

    const filteredSongs = songs.filter((s) =>
        s.title.toLowerCase().includes(search.toLowerCase())
    );

    const filteredPlaylists = playlists.filter((p) =>
        p.title.toLowerCase().includes(search.toLowerCase())
    );

    return (
        <div className="h-screen flex flex-col overflow-hidden bg-base-200">
            <header className="bg-base-100 shadow-sm border-b border-base-300 flex-shrink-0">
                <div className="navbar px-4 md:px-8 py-3">
                    <span className="text-xl font-bold text-primary ml-2 md:ml-0">
                        Music App
                    </span>
                </div>
            </header>

            <main className="flex-1 overflow-auto p-6 md:p-10 flex flex-col">
                <div className="flex justify-center gap-4 mb-4">
                    <button
                        className={`btn ${activeTab === "songs" ? "btn-primary" : "btn-outline"}`}
                        onClick={() => setActiveTab("songs")}
                    >
                        Songs
                    </button>
                    <button
                        className={`btn ${activeTab === "playlists" ? "btn-primary" : "btn-outline"}`}
                        onClick={() => setActiveTab("playlists")}
                    >
                        Playlists
                    </button>
                </div>

                <div className="flex justify-center mb-4">
                    <input
                        type="text"
                        placeholder={`Search ${activeTab}...`}
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        className="input input-bordered w-1/2"
                    />
                </div>

                <div className="flex-1 overflow-y-auto">
                    {activeTab === "songs" && (
                        <div className="grid gap-1 justify-center grid-cols-[repeat(auto-fit,minmax(200px,200px))]">
                            {filteredSongs.length === 0 && (
                                <div className="col-span-full text-center text-base-content/60">
                                    No songs found.
                                </div>
                            )}
                            {filteredSongs.map((song) => (
                                <div
                                    key={song.id}
                                    onClick={() => { setCurrentSong(song); setCurrentPlaylist(null); }}
                                    className="bg-base-100 rounded-lg shadow transition-transform transform hover:scale-105 flex flex-col items-center p-3 aspect-square w-full cursor-pointer group relative"
                                >
                                    {song.image ? (
                                        <img src={song.image} alt={song.title} className="w-full h-full object-cover rounded-md mb-2" />
                                    ) : (
                                        <div className="w-full h-full bg-base-300 rounded-md mb-2 flex items-center justify-center text-base-content/50">No Image</div>
                                    )}
                                    <div className="absolute inset-0 bg-black/30 rounded-lg opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                        <div className="w-12 h-12 rounded-full bg-primary flex items-center justify-center shadow-lg">
                                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" className="w-6 h-6 ml-1">
                                                <path d="M8 5v14l11-7z" />
                                            </svg>
                                        </div>
                                    </div>
                                    {isSongEditOn && (
                                        <button
                                            onClick={(e) => { e.stopPropagation(); navigate(`/editSong/${song.id}`); }}
                                            className="absolute bottom-2 right-2 btn btn-xs btn-primary opacity-0 group-hover:opacity-100 transition-opacity shadow-lg gap-1"
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="w-3 h-3">
                                                <path strokeLinecap="round" strokeLinejoin="round" d="M15.232 5.232l3.536 3.536M9 13l6.586-6.586a2 2 0 012.828 2.828L11.828 15.828a2 2 0 01-1.414.586H9v-2a2 2 0 01.586-1.414z" />
                                            </svg>
                                            Edit
                                        </button>
                                    )}
                                    <div className="font-semibold text-center text-sm mt-2">{song.title}</div>
                                </div>
                            ))}
                        </div>
                    )}

                    {activeTab === "playlists" && (
                        <div className="grid gap-1 justify-center grid-cols-[repeat(auto-fit,minmax(200px,200px))]">
                            {filteredPlaylists.length === 0 && (
                                <div className="col-span-full text-center text-base-content/60">
                                    No playlists found.
                                </div>
                            )}
                            {filteredPlaylists.map((playlist) => (
                                <div
                                    key={playlist.id}
                                    onClick={() => handlePlayPlaylist(playlist)}
                                    className="bg-base-100 rounded-lg shadow transition-transform transform hover:scale-105 flex flex-col items-center p-3 aspect-square w-full cursor-pointer group relative"
                                >
                                    {playlist.image ? (
                                        <img src={playlist.image} alt={playlist.title} className="w-full h-full object-cover rounded-md mb-2" />
                                    ) : (
                                        <div className="w-full h-full bg-base-300 rounded-md mb-2 flex items-center justify-center text-base-content/50">No Image</div>
                                    )}
                                    <div className="absolute inset-0 bg-black/30 rounded-lg opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                        <div className="w-12 h-12 rounded-full bg-primary flex items-center justify-center shadow-lg">
                                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" className="w-6 h-6 ml-1">
                                                <path d="M8 5v14l11-7z" />
                                            </svg>
                                        </div>
                                    </div>
                                    {isPlaylistEditOn && (
                                        <button
                                            onClick={(e) => { e.stopPropagation(); navigate(`/editPlaylist/${playlist.id}`); }}
                                            className="absolute bottom-2 right-2 btn btn-xs btn-primary opacity-0 group-hover:opacity-100 transition-opacity shadow-lg gap-1"
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="w-3 h-3">
                                                <path strokeLinecap="round" strokeLinejoin="round" d="M15.232 5.232l3.536 3.536M9 13l6.586-6.586a2 2 0 012.828 2.828L11.828 15.828a2 2 0 01-1.414.586H9v-2a2 2 0 01.586-1.414z" />
                                            </svg>
                                            Edit
                                        </button>
                                    )}
                                    <div className="font-semibold text-center text-sm mt-2 truncate w-full">{playlist.title}</div>
                                    <div className="text-xs text-base-content/50">{playlist.songs.length} songs</div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </main>
        </div>
    );
}