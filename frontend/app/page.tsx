
import Container from "@/components/container";
import GetAuctions from "@/components/api/auctions/getAuctions";

export default function Home() {
  return (
    <div>
        <Container>
          <GetAuctions />
        </Container>
    </div>
  );
}
