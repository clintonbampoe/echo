// A menu or popover opens under its trigger, lined up with the trigger's left edge. If that would push it past
// the right edge of the screen (a pill at the far right), it lines up with the right edge instead, so the page
// never grows sideways. Set directly on the element, as it only matters once.
export function keepOnScreen(el: HTMLElement | null) {
  if (
    !el ||
    el.getBoundingClientRect().right <= document.documentElement.clientWidth - 8
  )
    return;
  el.style.left = "auto";
  el.style.right = "0";
}
