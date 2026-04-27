import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import useMusicCrud from "../useMusicCrud";

export default function EditSong() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { getUserSongs, getSignedUrl, editSong, getFunctionState } = useMusicCrud();

    const [title, setTitle] = useState("");
    const [artist, setArtist] = useState("");
    const [isPublic, setIsPublic] = useState(false);
    const [image, setImage] = useState<File | null>(null);
    const [imagePreview, setImagePreview] = useState<string | null>(null);
    const [prevImgKey, setPrevImgKey] = useState<string | undefined>(undefined);
    const [isSaving, setIsSaving] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [isSongEditOn, setIsSongEditOn] = useState(false);

    const editSongFeatureKey = "edit_song";

    useEffect(() => {
        const loadData = async () => {
            const songs = await getUserSongs();
            const song = songs.find((s) => s.id === id);
            if (!song) return navigate("/myMusic");

            setTitle(song.title);
            setArtist(song.artist ?? "");
            setIsPublic(song.isPublic);

            if (song.image) {
                setPrevImgKey(song.image);
                const url = await getSignedUrl(song.image);
                setImagePreview(url);
            }

            const isEditOn = await getFunctionState(editSongFeatureKey);
            setIsSongEditOn(isEditOn);

            setIsLoading(false);
        };

        loadData();
    }, [id]);

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const selected = e.target.files?.[0] ?? null;
        setImage(selected);
        if (selected) {
            setImagePreview(URL.createObjectURL(selected));
        }
    };

    const handleSave = async () => {
        if (!id) return;
        try {
            setIsSaving(true);
            await editSong(id, title, artist, isPublic, prevImgKey, image ?? undefined);
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

    if (!isSongEditOn) {
        return (
            <div className="h-screen flex flex-col overflow-hidden bg-base-200">
                <header className="bg-base-100 shadow-sm border-b border-base-300 flex-shrink-0">
                    <div className="navbar px-4 md:px-8 py-3">
                        <div className="flex-1">
                            <span className="text-xl font-bold text-primary">Edit Song</span>
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
        <div className="h-full flex flex-col overflow-hidden bg-base-200">
            <header className="bg-base-100 shadow-sm border-b border-base-300 flex-shrink-0">
                <div className="navbar px-4 md:px-8 py-3">
                    <div className="flex-1">
                        <span className="text-xl font-bold text-primary">Edit Song</span>
                    </div>
                </div>
            </header>

            <main className="flex-1 flex items-center justify-center p-6 overflow-auto">
                <div className="card w-full max-w-lg bg-base-100 shadow-xl">
                    <div className="card-body">

                        <div className="flex justify-center mb-2">
                            <div className="relative w-36 h-36">
                                {imagePreview ? (
                                    <img
                                        src={imagePreview}
                                        alt="Cover"
                                        className="w-36 h-36 object-cover rounded-xl shadow"
                                    />
                                ) : (
                                    <div className="w-36 h-36 bg-base-300 rounded-xl flex items-center justify-center text-base-content/40 text-4xl">
                                        🎵
                                    </div>
                                )}
                                <label className="absolute inset-0 flex items-center justify-center bg-black/40 rounded-xl opacity-0 hover:opacity-100 transition-opacity cursor-pointer">
                                    <span className="text-white text-xs font-semibold">Change Image</span>
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
                                <span className="label-text">Song Title</span>
                            </label>
                            <input
                                type="text"
                                placeholder="Enter song title"
                                className="input input-bordered w-full"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                            />
                        </div>

                        <div className="form-control w-full mt-4">
                            <label className="label">
                                <span className="label-text">Artist</span>
                            </label>
                            <input
                                type="text"
                                placeholder="Enter artist name"
                                className="input input-bordered w-full"
                                value={artist}
                                onChange={(e) => setArtist(e.target.value)}
                            />
                        </div>

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
            </main>
        </div>
    );
}