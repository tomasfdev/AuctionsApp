import React from 'react'
import AuctionCard from './AuctionCard';
import { Auction, PagedResult } from '@/types';

async function getAuctions(): Promise<PagedResult<Auction>> {
    const result = await fetch('http://localhost:6001/search?pageSize=10');

    if (!result.ok) throw new Error('Failed to fetch data');

    return result.json();
}

export default async function Listings() {
    const auctions = await getAuctions();

    return (
        <div className='grid grid-cols-4 gap-6'>
            {auctions && auctions.results.map(auction => (
                <AuctionCard auction={auction} key={auction.id} />
            ))}
        </div>
    )
}
