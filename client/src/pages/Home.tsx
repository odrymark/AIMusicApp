import {useEffect, useRef, useState} from "react";
import { useSetAtom } from "jotai";
import { currentSongAtom } from "../atoms/currentSongAtom";
import { currentPlaylistAtom } from "../atoms/currentPlaylistAtom";
import SongSection from "../components/SongSection.tsx";
import useMusicCrud, {type Playlist, type Song} from "../useMusicCrud.ts";

export default function Home() {
    const topTrendingRef = useRef<HTMLDivElement>(null);
    const mostListenedRef = useRef<HTMLDivElement>(null);
    const recommendationsRef = useRef<HTMLDivElement>(null);
    const [songs, setSongs] = useState<Song[]>([]);
    const [playlists, setPlaylists] = useState<Playlist[]>([]);
    const [searchQuery, setSearchQuery] = useState("");
    const { getSongs, getSignedUrl, getPlaylists } = useMusicCrud();
    const setCurrentSong = useSetAtom(currentSongAtom);
    const setCurrentPlaylist = useSetAtom(currentPlaylistAtom);

    useEffect(() => {
        getSongs().then(async (res) => {
            const songsWithUrls = await Promise.all(
                res.map(async (song) => ({
                    ...song,
                    songUrl: await getSignedUrl(song.songKey),
                    image: song.image ? await getSignedUrl(song.image) : null,
                }))
            );
            setSongs(songsWithUrls);
        });

        getPlaylists().then(async (res) => {
            const playlistsWithUrls = await Promise.all(
                res.map(async (playlist) => ({
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
        });
    }, []);

    const handlePlayPlaylist = (playlist: Playlist) => {
        if (playlist.songs.length === 0) return;
        setCurrentPlaylist(playlist);
        setCurrentSong(playlist.songs[0]);
    };

    const handleSongClick = (song: Song) => {
        setCurrentPlaylist(null);
        setCurrentSong(song);
    };

    const filteredSongs = songs.filter((song) =>
        song.title.toLowerCase().includes(searchQuery.toLowerCase())
    );

    const isSearching = searchQuery.trim().length > 0;

    return (
        <div className="h-screen flex flex-col bg-base-200">
            <header className="bg-base-100 shadow-sm border-b border-base-300 flex-shrink-0">
                <div className="px-4 md:px-8 py-3 flex flex-col gap-2">
                    <span className="text-xl font-bold text-primary">Music App</span>
                    <div className="relative w-full max-w-sm">
                        <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-base-content/40" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-4.35-4.35M17 11A6 6 0 1 1 5 11a6 6 0 0 1 12 0z" />
                        </svg>
                        <input
                            type="text"
                            placeholder="Search songs..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            className="input input-bordered input-sm w-full pl-9 bg-base-200 focus:outline-none focus:border-primary"
                        />
                    </div>
                </div>
            </header>

            <div className="flex flex-1 overflow-hidden">
                <main className="flex-1 overflow-y-auto p-6 md:p-10">
                    <div className="max-w-6xl mx-auto">
                        {isSearching ? (
                            <>
                                <p className="text-sm text-base-content/50 mb-4">
                                    {filteredSongs.length} result{filteredSongs.length !== 1 ? "s" : ""} for "{searchQuery}"
                                </p>
                                <div className="grid gap-1 justify-center grid-cols-[repeat(auto-fit,minmax(200px,200px))]">
                                    {filteredSongs.length === 0 && (
                                        <div className="col-span-full text-center text-base-content/60">No songs found.</div>
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
                                            <div className="font-semibold text-center text-sm mt-2">{song.title}</div>
                                        </div>
                                    ))}
                                </div>
                            </>
                        ) : (
                            <>
                                <SongSection ref={topTrendingRef} title="Top Trending" songs={songs} onSongClick={handleSongClick} />
                                <SongSection ref={mostListenedRef} title="Your Most Listened" songs={songs} onSongClick={handleSongClick} />
                                <SongSection ref={recommendationsRef} title="Recommendations For You" songs={songs} onSongClick={handleSongClick} />
                            </>
                        )}
                    </div>
                </main>

                <aside className="w-72 border-l border-base-300 bg-base-100 p-6 flex-shrink-0 flex flex-col">
                    <h3 className="text-lg font-semibold mb-4 text-center text-primary">Top Playlists</h3>
                    <div className="flex-1 flex flex-col gap-3 overflow-y-auto">
                        {playlists.length === 0 && (
                            <p className="text-center text-base-content/40 text-sm">No playlists yet.</p>
                        )}
                        {playlists.map((playlist) => (
                            <div
                                key={playlist.id}
                                onClick={() => handlePlayPlaylist(playlist)}
                                className="flex items-center gap-3 cursor-pointer hover:bg-base-200 p-2 rounded group relative"
                            >
                                {playlist.image ? (
                                    <img src={playlist.image} alt={playlist.title} className="w-12 h-12 rounded-md object-cover flex-shrink-0" />
                                ) : (
                                    <div className="w-12 h-12 rounded-md bg-base-300 flex items-center justify-center text-xl flex-shrink-0">🎶</div>
                                )}
                                <div className="min-w-0 flex-1">
                                    <p className="font-medium truncate">{playlist.title}</p>
                                    <p className="text-xs text-base-content/50 truncate">{playlist.songs.length} songs</p>
                                </div>
                                <div className="w-8 h-8 rounded-full bg-primary flex items-center justify-center shadow opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" className="w-4 h-4 ml-0.5">
                                        <path d="M8 5v14l11-7z" />
                                    </svg>
                                </div>
                            </div>
                        ))}
                    </div>
                </aside>
            </div>
        </div>
    );
}