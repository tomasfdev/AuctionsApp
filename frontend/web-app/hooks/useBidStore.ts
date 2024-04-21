import { Bid } from "@/types";
import { create } from "zustand";

type State = {
  bids: Bid[];
};

type Actions = {
  setBids: (bids: Bid[]) => void;
  addBid: (bid: Bid) => void;
};

export const useBidStore = create<State & Actions>((set) => ({
  bids: [],

  setBids: (bids: Bid[]) => {
    set(() => ({
      bids,
    }));
  },

  addBid: (bid: Bid) => {
    set((state) => ({
      bids: !state.bids.find((x) => x.id === bid.id) //check if already have the same bid with the same id already in the store
        ? [bid, ...state.bids] //add bid to the front and the rest of the bids inside the store are going to be added after the new bid
        : [...state.bids], //if bid already exits in the store, don't add bid and add the rest of the bids inside the store
    }));
  },
}));
