export default async function ApiTest() {
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/test`, {
    cache: "no-store",
  });
  
  const data = await res.text();

  return (
    <div>
      <p>{data}</p>
    </div>
  );
}
