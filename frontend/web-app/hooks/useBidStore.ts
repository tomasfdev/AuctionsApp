import { Bid } from "@/types";
import { create } from "zustand";
import { shallow } from "zustand/shallow";
import { createWithEqualityFn } from "zustand/traditional";

type State = {
  auctionId: string | null;
  bids: Bid[];
  open: boolean;
};

type Actions = {
  setBids: (auctionId: string, bids: Bid[]) => void;
  addBid: (bid: Bid) => void;
  setOpen: (value: boolean) => void;
};

export const useBidStore = createWithEqualityFn<State & Actions>((set) => ({
  auctionId: null,
  bids: [],
  open: true,

  setBids: (auctionId: string, bids: Bid[]) => {
    set(() => ({
      auctionId,
      bids,
    }));
  },

  addBid: (bid: Bid) => {
    set((state) => {
      if (state.auctionId && bid.auctionId === state.auctionId) {
        return {
          bids: !state.bids.find((x) => x.id === bid.id) //check if already have the same bid with the same id already in the store
            ? [bid, ...state.bids] //add bid to the front and the rest of the bids inside the store are going to be added after the new bid
            : [...state.bids], //if bid already exits in the store, don't add bid and add the rest of the bids inside the store
        };
      }
      return state;
    });
  },

  setOpen: (value: boolean) => {
    set(() => ({
      open: value,
    }));
  },
  shallow,
}));
