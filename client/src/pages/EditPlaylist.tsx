import { useState, useEffect, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import useMusicCrud, { type Song } from "../useMusicCrud";

export default function EditPlaylist() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { getUserPlaylists, getSongs, getSignedUrl, editPlaylist, getFunctionState } = useMusicCrud();

    const [title, setTitle] = useState("");
    const [isPublic, setIsPublic] = useState(false);
    const [image, setImage] = useState<File | null>(null);
    const [imagePreview, setImagePreview] = useState<string | null>(null);
    const [prevImgKey, setPrevImgKey] = useState<string | undefined>(undefined);
    const [orderedSelected, setOrderedSelected] = useState<Song[]>([]);
    const [allSongs, setAllSongs] = useState<Song[]>([]);
    const [search, setSearch] = useState("");
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);

    const [isPlaylistEditOn, setIsPlaylistEditOn] = useState(false);
    const editPlaylistFeatureKey = "edit_playlist";

    const [draggingIndex, setDraggingIndex] = useState<number | null>(null);
    const dragState = useRef<{
        fromIndex: number;
        startY: number;
        currentY: number;
        itemHeight: number;
    } | null>(null);

    useEffect(() => {
        const loadData = async () => {
            const [playlists, songs] = await Promise.all([getUserPlaylists(), getSongs()]);
            const playlist = playlists.find((p) => p.id === id);
            if (!playlist) return navigate("/myMusic");

            setTitle(playlist.title);
            setIsPublic(playlist.isPublic);

            if (playlist.image) {
                setPrevImgKey(playlist.image);
                const url = await getSignedUrl(playlist.image);
                setImagePreview(url);
            }

            const songsWithUrls = await Promise.all(
                songs.map(async (song) => ({
                    ...song,
                    image: song.image ? await getSignedUrl(song.image) : null,
                }))
            );
            setAllSongs(songsWithUrls);

            const playlistSongIds: string[] = playlist.songs.map((s: Song) => s.id);
            const preSelected = playlistSongIds
                .map((sid) => songsWithUrls.find((s) => s.id === sid))
                .filter(Boolean) as Song[];
            setOrderedSelected(preSelected);

            const isEditOn = await getFunctionState(editPlaylistFeatureKey);
            setIsPlaylistEditOn(isEditOn);

            setIsLoading(false);
        };

        loadData();
    }, [id]);

    const selectedIds = new Set(orderedSelected.map((s) => s.id));

    const filteredSongs = allSongs.filter(
        (s) =>
            s.title.toLowerCase().includes(search.toLowerCase()) ||
            s.artist.toLowerCase().includes(search.toLowerCase())
    );

    const toggleSong = (song: Song) => {
        setOrderedSelected((prev) =>
            prev.some((s) => s.id === song.id)
                ? prev.filter((s) => s.id !== song.id)
                : [...prev, song]
        );
    };

    const onPointerDown = (e: React.PointerEvent, index: number) => {
        const item = e.currentTarget as HTMLElement;
        item.setPointerCapture(e.pointerId);
        dragState.current = {
            fromIndex: index,
            startY: e.clientY,
            currentY: e.clientY,
            itemHeight: item.getBoundingClientRect().height + 4,
        };
        setDraggingIndex(index);
    };

    const onPointerMove = (e: React.PointerEvent) => {
        if (!dragState.current) return;
        dragState.current.currentY = e.clientY;
        const delta = dragState.current.currentY - dragState.current.startY;
        const steps = Math.round(delta / dragState.current.itemHeight);
        const newIndex = Math.max(
            0,
            Math.min(orderedSelected.length - 1, dragState.current.fromIndex + steps)
        );
        if (newIndex !== draggingIndex) {
            setOrderedSelected((prev) => {
                const next = [...prev];
                const [moved] = next.splice(draggingIndex!, 1);
                next.splice(newIndex, 0, moved);
                return next;
            });
            setDraggingIndex(newIndex);
        }
    };

    const onPointerUp = () => {
        dragState.current = null;
        setDraggingIndex(null);
    };

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const selected = e.target.files?.[0] ?? null;
        setImage(selected);
        if (selected) setImagePreview(URL.createObjectURL(selected));
    };

    const handleSave = async () => {
        if (!id) return;
        if (!title.trim()) return alert("Please enter a playlist title.");
        if (orderedSelected.length === 0) return alert("Please select at least one song.");
        try {
            setIsSaving(true);
            await editPlaylist(id, title, orderedSelected.map((s) => s.id), isPublic, prevImgKey, image ?? undefined);
            navigate("/myMusic");
        } catch {
            alert("Failed to save changes.");
        } finally {
            setIsSaving(false);
        }
    };

    if (isLoading) {
        return (
            <div className="h-full flex items-center justify-center">
                <span className="loading loading-spinner loading-lg text-primary" />
            </div>
        );
    }

    if (!isPlaylistEditOn) {
        return (
            <div className="h-screen flex flex-col overflow-hidden bg-base-200">
                <header className="bg-base-100 shadow-sm border-b border-base-300 flex-shrink-0">
                    <div className="navbar px-4 md:px-8 py-3">
                        <div className="flex-1">
                            <span className="text-xl font-bold text-primary">Edit Playlist</span>
                        </div>
                    </div>
                </header>
                <main className="flex-1 flex items-center justify-center">
                    <div className="text-center">
                        <p className="text-2xl font-semibold text-base-content">This functionality is currently not available</p>
                    </div>
                </main>
            </div>
        );
    }

    return (
        <div className="h-screen flex flex-col overflow-hidden bg-base-200">
            <header className="bg-base-100 shadow-sm border-b border-base-300 flex-shrink-0">
                <div className="navbar px-4 md:px-8 py-3">
                    <div className="flex-1">
                        <span className="text-xl font-bold text-primary">Edit Playlist</span>
                    </div>
                </div>
            </header>

            <main className="flex-1 overflow-hidden p-6">
                <div className="flex gap-6 h-full max-w-5xl mx-auto">

                    <div className="card bg-base-100 shadow-xl w-80 flex-shrink-0 overflow-y-auto">
                        <div className="card-body">
                            <h2 className="card-title text-primary">Playlist Details</h2>

                            <div className="flex justify-center mt-2 mb-2">
                                <div className="relative w-36 h-36">
                                    {imagePreview ? (
                                        <img
                                            src={imagePreview}
                                            alt="Cover"
                                            className="w-36 h-36 object-cover rounded-xl shadow"
                                        />
                                    ) : (
                                        <div className="w-36 h-36 bg-base-300 rounded-xl flex items-center justify-center text-base-content/40 text-4xl">
                                            🎶
                                        </div>
                                    )}
                                    <label className="absolute inset-0 flex items-center justify-center bg-black/40 rounded-xl opacity-0 hover:opacity-100 transition-opacity cursor-pointer">
                                        <span className="text-white text-xs font-semibold">
                                            {imagePreview ? "Change Image" : "Add Image"}
                                        </span>
                                        <input
                                            type="file"
                                            accept="image/jpeg,image/png,image/webp"
                                            className="hidden"
                                            onChange={handleImageChange}
                                        />
                                    </label>
                                </div>
                            </div>

                            {image && (
                                <p className="text-center text-xs text-base-content/50 -mt-1 mb-1">
                                    New image selected: {image.name}
                                </p>
                            )}

                            <div className="form-control w-full mt-2">
                                <label className="label">
                                    <span className="label-text">Playlist Title</span>
                                </label>
                                <input
                                    type="text"
                                    placeholder="Enter playlist title"
                                    className="input input-bordered w-full"
                                    value={title}
                                    onChange={(e) => setTitle(e.target.value)}
                                />
                            </div>

                            {orderedSelected.length > 0 && (
                                <div className="mt-4">
                                    <label className="label pt-0">
                                        <span className="label-text">Song Order</span>
                                        <span className="label-text-alt text-base-content/40">drag to reorder</span>
                                    </label>
                                    <div className="flex flex-col gap-1">
                                        {orderedSelected.map((song, index) => (
                                            <div
                                                key={song.id}
                                                onPointerDown={(e) => onPointerDown(e, index)}
                                                onPointerMove={onPointerMove}
                                                onPointerUp={onPointerUp}
                                                onPointerCancel={onPointerUp}
                                                className={`flex items-center gap-2 bg-base-200 rounded-lg px-2 py-1.5 cursor-grab active:cursor-grabbing select-none transition-all ${
                                                    draggingIndex === index
                                                        ? "opacity-50 scale-95 shadow-lg z-10"
                                                        : ""
                                                }`}
                                            >
                                                <span className="text-base-content/30 text-sm w-4 text-center flex-shrink-0">⠿</span>
                                                <span className="text-xs text-base-content/40 w-4 flex-shrink-0">{index + 1}</span>
                                                {song.image ? (
                                                    <img
                                                        src={song.image}
                                                        alt={song.title}
                                                        className="w-7 h-7 rounded object-cover flex-shrink-0"
                                                    />
                                                ) : (
                                                    <div className="w-7 h-7 rounded bg-base-300 flex items-center justify-center text-xs flex-shrink-0">🎵</div>
                                                )}
                                                <div className="flex flex-col min-w-0">
                                                    <span className="text-xs font-medium truncate">{song.title}</span>
                                                    <span className="text-xs text-base-content/40 truncate">{song.artist}</span>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}

                            <div className="form-control mt-4">
                                <label className="label cursor-pointer justify-start gap-4">
                                    <span className="label-text">Visible to Others</span>
                                    <input
                                        type="checkbox"
                                        className="toggle toggle-primary"
                                        checked={isPublic}
                                        onChange={(e) => setIsPublic(e.target.checked)}
                                    />
                                </label>
                            </div>

                            <div className="card-actions justify-between mt-6">
                                <button
                                    className="btn btn-ghost"
                                    onClick={() => navigate("/myMusic")}
                                >
                                    Cancel
                                </button>
                                <button
                                    className={`btn btn-primary ${isSaving ? "loading" : ""}`}
                                    onClick={handleSave}
                                    disabled={isSaving}
                                >
                                    {isSaving ? "Saving..." : "Save Changes"}
                                </button>
                            </div>
                        </div>
                    </div>

                    <div className="card bg-base-100 shadow-xl flex-1 overflow-hidden flex flex-col">
                        <div className="card-body flex flex-col overflow-hidden">
                            <div className="flex items-center justify-between flex-shrink-0">
                                <h3 className="font-semibold text-lg text-primary">Songs</h3>
                                {orderedSelected.length > 0 && (
                                    <span className="badge badge-primary">{orderedSelected.length} selected</span>
                                )}
                            </div>

                            <input
                                type="text"
                                placeholder="Search by title or artist..."
                                className="input input-bordered w-full mt-2 flex-shrink-0"
                                value={search}
                                onChange={(e) => setSearch(e.target.value)}
                            />

                            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3 mt-4 overflow-y-auto flex-1 pr-1">
                                {filteredSongs.length === 0 && (
                                    <p className="col-span-full text-center text-base-content/40 py-8">No songs found.</p>
                                )}
                                {filteredSongs.map((song) => {
                                    const selected = selectedIds.has(song.id);
                                    return (
                                        <div
                                            key={song.id}
                                            onClick={() => toggleSong(song)}
                                            className={`cursor-pointer rounded-xl border-2 p-2 transition-all ${
                                                selected
                                                    ? "border-primary bg-primary/10"
                                                    : "border-base-300 hover:border-primary/50"
                                            }`}
                                        >
                                            <div className="relative">
                                                {song.image ? (
                                                    <img
                                                        src={song.image}
                                                        alt={song.title}
                                                        className="w-full aspect-square object-cover rounded-lg"
                                                    />
                                                ) : (
                                                    <div className="w-full aspect-square bg-base-300 rounded-lg flex items-center justify-center text-3xl">🎵</div>
                                                )}
                                                {selected && (
                                                    <div className="absolute inset-0 bg-primary/20 rounded-lg flex items-center justify-center">
                                                        <span className="text-primary text-2xl font-bold">✓</span>
                                                    </div>
                                                )}
                                            </div>
                                            <p className="text-sm font-semibold mt-2 truncate">{song.title}</p>
                                            <p className="text-xs text-base-content/50 truncate">{song.artist}</p>
                                        </div>
                                    );
                                })}
                            </div>
                        </div>
                    </div>

                </div>
            </main>
        </div>
    );
}