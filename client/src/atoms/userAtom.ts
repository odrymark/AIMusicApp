import { atom } from "jotai";
import { type UserInfo } from "../useMusicCrud";

export const userAtom = atom<UserInfo | null>(null);