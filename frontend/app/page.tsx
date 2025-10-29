
import Container from "@/components/container";
import GetAuctions from "@/components/api/auctions/getAuctions";
import PostAuction from "@/components/api/auctions/postAuction";
import ExampleComponent from "@/components/exampleComponent";

export default function Home() {
  return (
    <div>
        <Container>
          <ExampleComponent />
          <GetAuctions />
          <PostAuction />
        </Container>
    </div>
  );
}
