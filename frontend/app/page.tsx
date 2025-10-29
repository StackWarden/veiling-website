import GetAuctions from "@/components/api/auctions/getAuctions";
import PostAuction from "@/components/api/auctions/postAuction";
import ExampleComponent from "@/components/exampleComponent";

export default function Home() {
  return (
    <div>
      <main>
        <ExampleComponent />
        <GetAuctions />
        <PostAuction />
      </main>
    </div>
  );
}
