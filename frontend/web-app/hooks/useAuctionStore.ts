import { Auction, PagedResult } from "@/types";
import { create } from "zustand";

type State = {
  auctions: Auction[];
  totalCount: number;
  pageCount: number;
};

type Actions = {
  setData: (data: PagedResult<Auction>) => void;
  setCurrentPrice: (auctionId: string, amount: number) => void; //update the price for any auction inside auctions[] when receive an accepted bid back from the SignalR server
};

const initialState: State = {
  auctions: [],
  pageCount: 0,
  totalCount: 0,
};

//AuctionStore hook
export const useAuctionStore = create<State & Actions>((set) => ({
  ...initialState, //store config

  setData: (data: PagedResult<Auction>) => {
    set(() => ({
      auctions: data.results,
      totalCount: data.totalCount,
      pageCount: data.pageCount,
    }));
  },

  setCurrentPrice: (auctionId: string, amount: number) => {
    set((state) => ({
      auctions: state.auctions.map((auction) =>
        auction.id === auctionId
          ? { ...auction, currentHighBid: amount }
          : auction
      ),
    }));
  },
}));
