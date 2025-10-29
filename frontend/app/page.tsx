
import Container from "@/components/container";
import GetAuctions from "@/components/api/auctions/getAuctions";
import PostAuction from "@/components/api/auctions/postAuction";

export default function Home() {
  return (
    <div>
        <Container>
          <GetAuctions />
          <PostAuction />
        </Container>
    </div>
  );
}
