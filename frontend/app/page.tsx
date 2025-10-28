import AuctionList from "@/components/getAuctions";
import Container from "@/components/container";

export default function Home() {
  return (
    <div>
        <Container>
          <AuctionList />
        </Container>
    </div>
  );
}
