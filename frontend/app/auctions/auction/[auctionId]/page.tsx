import Header from "@/components/header";
import AuctionScreen from "@/components/auction/AuctionScreen";

type Props = {
  params: {
    auctionId: string;
  };
};

export default async function AuctionPage({ params }: Props) {
  return (
    <>
      <Header />
      <main className="w-full px-20 py-8">
        <AuctionScreen auctionId={params.auctionId} />
      </main>
    </>
  );
}
