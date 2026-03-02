// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
  function prepareResponsiveTables() {
    const tables = document.querySelectorAll(".app-table");

    tables.forEach((table) => {
      const headers = Array.from(table.querySelectorAll("thead th"))
        .map((th) => th.textContent?.trim() ?? "")
        .filter((text) => text.length > 0);

      if (!headers.length) return;

      table.classList.add("app-table-mobile-ready");

      const rows = table.querySelectorAll("tbody tr");
      rows.forEach((row) => {
        const cells = row.querySelectorAll("td");

        if (cells.length === 1 && cells[0].hasAttribute("colspan")) {
          cells[0].setAttribute("data-label", "");
          return;
        }

        cells.forEach((cell, index) => {
          if (cell.hasAttribute("data-label")) return;
          const header = headers[Math.min(index, headers.length - 1)] ?? "";
          cell.setAttribute("data-label", header);
        });
      });
    });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", prepareResponsiveTables);
  } else {
    prepareResponsiveTables();
  }
})();
