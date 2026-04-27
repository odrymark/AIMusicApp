import { atom } from "jotai";
import { type Playlist } from "../useMusicCrud";

export const currentPlaylistAtom = atom<Playlist | null>(null);