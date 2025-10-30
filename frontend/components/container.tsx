interface ContainerProps {
  children: React.ReactNode;
}

export default function Container(props: ContainerProps) {
  return (
    <main className="flex items-center justify-center w-screen h-screen bg-gray-100">
      <section
        className="
          bg-white p-8 rounded-2xl shadow-lg
          w-auto max-w-full
          sm:min-w-[400px] md:min-w-[500px]
          min-h-[500px]
          flex flex-col justify-center
        "
      >
        {props.children}
      </section>
    </main>
  );
}