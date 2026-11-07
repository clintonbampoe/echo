// A heading whose words arrive one after another. The full text is on the element for screen
// readers; the pieces are decoration. CSS does the timing from each word's --i.
export function WordReveal({ text }: { text: string }) {
  const words = text.split(" ");
  return (
    <>
      <span className="sr-only">{text}</span>
      <span aria-hidden="true">
        {words.map((word, i) => (
          <span
            key={i}
            className="word-reveal"
            style={{ ["--i" as string]: i }}
          >
            {word}
            {i < words.length - 1 ? " " : ""}
          </span>
        ))}
      </span>
    </>
  );
}
