(function () {
  var storageKey = "lizerium.localization.docs.language";

  function getBrowserLanguage() {
    var languages = navigator.languages && navigator.languages.length
      ? navigator.languages
      : [navigator.language || navigator.userLanguage || "en"];

    return String(languages[0] || "en").toLowerCase().indexOf("ru") === 0
      ? "ru"
      : "en";
  }

  function getOptionLanguage(option) {
    var explicit = option.getAttribute("data-lang");
    if (explicit) {
      return explicit;
    }

    return String(option.text || "").toLowerCase().indexOf("рус") >= 0
      ? "ru"
      : "en";
  }

  function getCurrentLanguage(select) {
    var selected = select.options[select.selectedIndex];
    return selected ? getOptionLanguage(selected) : "en";
  }

  function getSavedLanguage() {
    try {
      var saved = localStorage.getItem(storageKey);
      return saved === "ru" || saved === "en" ? saved : null;
    } catch (e) {
      return null;
    }
  }

  function saveLanguage(language) {
    try {
      localStorage.setItem(storageKey, language);
    } catch (e) {
    }
  }

  function findTargetOption(select, language) {
    for (var i = 0; i < select.options.length; i += 1) {
      if (getOptionLanguage(select.options[i]) === language) {
        return select.options[i];
      }
    }

    return null;
  }

  function redirectIfNeeded(select) {
    var desired = getSavedLanguage() || getBrowserLanguage();
    var current = getCurrentLanguage(select);
    if (desired === current) {
      return;
    }

    var option = findTargetOption(select, desired);
    var target = option && option.getAttribute("data-url");
    if (target) {
      window.location.replace(target);
    }
  }

  function initLanguageSelect(select) {
    redirectIfNeeded(select);

    select.addEventListener("change", function () {
      var selected = select.options[select.selectedIndex];
      var target = selected.getAttribute("data-url");
      saveLanguage(getOptionLanguage(selected));
      if (target) {
        window.location.href = target;
      }
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    Array.prototype.forEach.call(
      document.querySelectorAll("[data-language-select]"),
      initLanguageSelect);
  });
})();
