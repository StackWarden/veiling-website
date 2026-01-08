import Header from "@/components/header";
import AuctionScreen from "@/components/auction/AuctionScreen";

type Props = {
  params: Promise<{
    auctionId: string;
  }>;
};

export default async function AuctionPage({ params }: Props) {
  const { auctionId } = await params;
  return (
    <>
      <Header />
      <main className="w-full px-20 py-8">
        <AuctionScreen auctionId={auctionId} />
      </main>
    </>
  );
}
