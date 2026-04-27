import { forwardRef } from "react";
import type { Song } from "../useMusicCrud.ts";

interface SongSectionProps {
    title: string;
    songs: Song[];
    onSongClick?: (song: Song) => void;
}

const SongSection = forwardRef<HTMLDivElement, SongSectionProps>(
    ({ title, songs, onSongClick }, ref) => {
        const scrollRow = (direction: "left" | "right") => {
            if (ref && "current" in ref && ref.current) {
                ref.current.scrollBy({
                    left: direction === "left" ? -300 : 300,
                    behavior: "smooth",
                });
            }
        };

        return (
            <div className="mb-12">
                <h2 className="text-2xl font-semibold mb-4 text-primary">{title}</h2>

                <div className="relative">
                    <button
                        onClick={() => scrollRow("left")}
                        className="absolute left-0 top-1/2 -translate-y-1/2 z-10 bg-base-100/80 hover:bg-base-100 p-2 rounded-full shadow"
                    >
                        ◀
                    </button>

                    <div
                        ref={ref}
                        className="flex gap-4 overflow-hidden scroll-smooth px-12"
                    >
                        {songs.length === 0 ? (
                            <div className="w-full flex justify-center items-center py-8 text-base-content/50 italic">
                                No songs found
                            </div>
                        ) : (
                            songs.map((song) => (
                                <div
                                    key={song.id}
                                    onClick={() => onSongClick?.(song)}
                                    className="flex-shrink-0 w-40 bg-base-100 rounded-lg shadow transition-transform transform hover:scale-105 flex flex-col items-center p-3 aspect-square cursor-pointer group relative"
                                >
                                    {song.image ? (
                                        <img
                                            src={song.image}
                                            alt={song.title}
                                            className="w-full h-full object-cover rounded-md mb-2"
                                        />
                                    ) : (
                                        <div className="w-full h-full bg-base-300 rounded-md mb-2 flex items-center justify-center text-base-content/50">
                                            No Image
                                        </div>
                                    )}

                                    <div className="absolute inset-0 bg-black/30 rounded-lg opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                        <div className="w-12 h-12 rounded-full bg-primary flex items-center justify-center shadow-lg">
                                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" className="w-6 h-6 ml-1">
                                                <path d="M8 5v14l11-7z" />
                                            </svg>
                                        </div>
                                    </div>

                                    <div className="font-semibold text-center text-sm mt-2">
                                        {song.title}
                                    </div>
                                </div>
                            ))
                        )}
                    </div>

                    <button
                        onClick={() => scrollRow("right")}
                        className="absolute right-0 top-1/2 -translate-y-1/2 z-10 bg-base-100/80 hover:bg-base-100 p-2 rounded-full shadow"
                    >
                        ▶
                    </button>
                </div>
            </div>
        );
    }
);

export default SongSection;