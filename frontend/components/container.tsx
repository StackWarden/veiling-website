interface ContainerProps {
  children: React.ReactNode;
}

export default function Container(props: ContainerProps) {
  return (
    <main className="flex items-center justify-center w-screen h-screen bg-gray-100">
      <section
        className="
          bg-white p-8 rounded-2xl shadow-lg
          w-full max-w-md sm:max-w-lg md:max-w-xl
          min-h-[500px]
          flex flex-col justify-center
        "
      >
        {props.children}
      </section>
    </main>
  );
}