"use server";

import { Auction, PagedResult } from "@/types";
import { getTokenWorkaround } from "./authActions";

export async function getAuctions(
  query: string
): Promise<PagedResult<Auction>> {
  const result = await fetch(`http://localhost:6001/search${query}`);

  if (!result.ok) throw new Error("Failed to fetch data");

  return result.json();
}

export async function UpdateAuctionTest() {
  const data = { mileage: Math.floor(Math.random() * 100000) + 1 };

  const token = await getTokenWorkaround();

  const response = await fetch(
    "http://localhost:6001/auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c",
    {
      method: "PUT",
      headers: {
        "Content-type": "application/json",
        Authorization: "Bearer " + token?.access_token,
      },
      body: JSON.stringify(data),
    }
  );

  if (!response.ok)
    return { status: response.status, message: response.statusText };

  return response.statusText;
}
