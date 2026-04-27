import { useState } from "react";
import useMusicCrud from "../useMusicCrud";

export default function UploadSong() {
    const { uploadSong } = useMusicCrud();

    const [file, setFile] = useState<File | null>(null);
    const [image, setImage] = useState<File | null>(null);
    const [imagePreview, setImagePreview] = useState<string | null>(null);
    const [title, setTitle] = useState("");
    const [artist, setArtist] = useState("");
    const [isPublic, setIsPublic] = useState(false);
    const [isUploading, setIsUploading] = useState(false);

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const selected = e.target.files?.[0] ?? null;
        setImage(selected);
        if (selected) {
            setImagePreview(URL.createObjectURL(selected));
        } else {
            setImagePreview(null);
        }
    };

    const handleUpload = async () => {
        if (!file) {
            alert("Please select a song file.");
            return;
        }

        try {
            setIsUploading(true);
            await uploadSong(file, title, artist, isPublic, image ?? undefined);
            alert("Song uploaded successfully!");
            setFile(null);
            setImage(null);
            setImagePreview(null);
            setTitle("");
            setArtist("");
            setIsPublic(false);
        } catch (error) {
            alert("Upload failed.");
        } finally {
            setIsUploading(false);
        }
    };

    return (
        <div className="h-screen flex flex-col overflow-hidden bg-base-200">
            <header className="bg-base-100 shadow-sm border-b border-base-300 flex-shrink-0">
                <div className="navbar px-4 md:px-8 py-3">
                    <div className="flex-1">
                        <span className="text-xl font-bold text-primary ml-2 md:ml-0">
                            Music App
                        </span>
                    </div>
                </div>
            </header>

            <main className="flex-1 flex items-center justify-center p-6">
                <div className="card w-full max-w-lg bg-base-100 shadow-xl">
                    <div className="card-body">
                        <h2 className="card-title text-primary">
                            Upload New Song
                        </h2>

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

                        <div className="form-control w-full mt-4">
                            <label className="label">
                                <span className="label-text">Select MP3 File</span>
                            </label>
                            <input
                                type="file"
                                accept="audio/mpeg"
                                className="file-input file-input-bordered w-full"
                                onChange={(e) =>
                                    setFile(e.target.files ? e.target.files[0] : null)
                                }
                            />
                        </div>

                        <div className="form-control w-full mt-4">
                            <label className="label">
                                <span className="label-text">Cover Image <span className="text-base-content/40">(optional)</span></span>
                            </label>
                            <div className="flex justify-center mb-2">
                                <div className="relative w-36 h-36">
                                    {imagePreview ? (
                                        <img
                                            src={imagePreview}
                                            alt="Cover preview"
                                            className="w-36 h-36 object-cover rounded-xl shadow"
                                        />
                                    ) : (
                                        <div className="w-36 h-36 bg-base-300 rounded-xl flex items-center justify-center text-base-content/40 text-4xl">
                                            🎵
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
                                <p className="text-center text-xs text-base-content/50 mb-1">
                                    Selected: {image.name}
                                </p>
                            )}
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

                        <div className="card-actions justify-end mt-6">
                            <button
                                className={`btn btn-primary ${isUploading ? "loading" : ""}`}
                                onClick={handleUpload}
                                disabled={isUploading}
                            >
                                {isUploading ? "Uploading..." : "Upload Song"}
                            </button>
                        </div>
                    </div>
                </div>
            </main>
        </div>
    );
}