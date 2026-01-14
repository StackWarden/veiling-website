import Header from "@/components/header";
import SetAuctionTimeScreen from "@/components/auction/SetAuctionTimeScreen";

type Props = {
  params: Promise<{
    auctionId: string;
  }>;
};

export default async function SetAuctionTimePage({ params }: Props) {
  const { auctionId } = await params;

  return (
    <>
      <Header />
      <main className="w-full px-20 py-8">
        <SetAuctionTimeScreen auctionId={auctionId} />
      </main>
    </>
  );
}
