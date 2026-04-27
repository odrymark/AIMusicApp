import { atom } from "jotai";
import { type Song } from "../useMusicCrud";

export const currentSongAtom = atom<Song | null>(null);