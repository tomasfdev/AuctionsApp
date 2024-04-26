"use client";

import { useAuctionStore } from "@/hooks/useAuctionStore";
import { useBidStore } from "@/hooks/useBidStore";
import { Auction, AuctionFinished, Bid } from "@/types";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { User } from "next-auth";
import React, { ReactNode, useEffect, useState } from "react";
import toast from "react-hot-toast";
import AuctionCreatedToast from "../components/AuctionCreatedToast";
import { getDetailedViewData } from "../actions/auctionActions";
import AuctionFinishedToast from "../components/AuctionFinishedToast";

type Props = {
  //props we're going to receive
  children: ReactNode;
  user: User | null;
};

export default function SignalRProvider({ children, user }: Props) {
  //setting up connection to SignalR
  const [connection, setConnection] = useState<HubConnection | null>(null); //store connection inside state
  const setCurrentPrice = useAuctionStore((state) => state.setCurrentPrice);
  const addBid = useBidStore((state) => state.addBid);

  //to make a connection to signalR Hub when the user loads the app
  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl(process.env.NEXT_PUBLIC_NOTIFY_URL!)
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection); //store the connection inside local state
  }, []);

  useEffect(() => {
    if (connection) {
      connection
        .start() //start connection
        .then(() => {
          console.log("Connected to notifications hub");
          //Listening/Subscribe to the hub method event "BidPlaced" in NotificationService/Consumers/BidPlacedConsumer, so when "BidPlaced" is executed in the server side, we can do something inside the client side
          connection.on("BidPlaced", (bid: Bid) => {
            //what to to when received a bid from the server side
            //console.log("Bid placed event received");
            //check if bidStatus is "Accepted" because we only do something/send a notification if the bid is "Accepted"
            if (bid.bidStatus.includes("Accepted")) {
              setCurrentPrice(bid.auctionId, bid.amount); //update the current highBid inside the auction
            }
            addBid(bid); //update the bid
          });

          connection.on("AuctionCreated", (auction: Auction) => {
            //show the toast of the new auction created to everybory except the seller/owner of the new auction
            if (user?.username !== auction.seller) {
              return toast(<AuctionCreatedToast auction={auction} />, {
                duration: 10000,
              });
            }
          });

          connection.on(
            "AuctionFinished",
            (auctionFinished: AuctionFinished) => {
              const auction = getDetailedViewData(auctionFinished.auctionId);
              return toast.promise(
                auction,
                {
                  loading: "Loading",
                  success: (auction) => (
                    <AuctionFinishedToast
                      auctionFinished={auctionFinished}
                      auction={auction}
                    />
                  ),
                  error: (err) => "Auction finished!",
                },
                { success: { duration: 10000, icon: null } }
              );
            }
          );
        })
        .catch((error) => console.log(error));
    }

    return () => {
      connection?.stop(); //stop the connection to the SignalR Server when this component is disposed of
    };
  }, [connection, setCurrentPrice]); //useEffect() dependencies

  return children;
}
