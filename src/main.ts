import "./style.css";

const root = document.querySelector("#app");
if (root) {
  const note = document.createElement("p");
  note.textContent = "Chamber Crawl boot OK";
  root.appendChild(note);
}
