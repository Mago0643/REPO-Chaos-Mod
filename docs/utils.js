let fuck = function(text, posInfo) {
  let objectString = text.toString();
  if (posInfo != null) {
    objectString += " (";
    objectString += posInfo;
    objectString += ")";
  }
  return objectString;
}

function log(object, posInfo) {
  console.log(fuck(object, posInfo));
}

function logError(object, posInfo) {
  console.error(fuck(object, posInfo));
}

function logWarn(object, posInfo) {
  console.warn(fuck(object, posInfo));
}

function logInfo(object, posInfo) {
  console.info(fuck(object, posInfo));
}

function logDebug(object, posInfo) {
  console.debug(fuck(object, posInfo));
}

/**
 * Formats a weird-looking numbers to cool money format.
 * 
 * Like, `10000000` will be `10,000,000`
 * @param {number} num Money number.
 */
function formatMoney(num) {
  return num.toLocaleString();
}

function clamp(v, min, max) {
  return Math.max(Math.min(v, max), min);
}

/**
 * formats number-time to cool fancy looking string.
 * 
 * Like, `120.000` will be `2:00`
 * @param {number} seconds 
 */
function formatTime(seconds) {
  seconds = Math.max(0, seconds);
  let sec = seconds % 60;
  let min = Math.floor(seconds / 60) % 60;
  let hour = Math.floor(seconds / 3600) % 60;
  let timeHelper = "";
  
  if (hour > 0) {
    timeHelper += hour;
    timeHelper += ":";
    if (min < 10)
      timeHelper += "0";
  }
  timeHelper += min;
  timeHelper += ":";
  if (sec < 10)
    timeHelper += "0";
  timeHelper += sec;
  return timeHelper;
}

function getLang() {
  const params = new URLSearchParams(window.location.search);
  return params.get('lang') || 'en';
}

function formatLang(lang) {
  switch (lang) {
    case 'en-us' || 'us-en':
      return "en";
    case 'ko-kr' || 'kr-ko':
      return "ko";
  }
  return lang;
}

/**
 * Replaces All Characters in string.
 * @param {string} str string
 * @param {string} find want to change
 * @param {string} to after
 */
function replaceAll(str, find, to) {
  let sex = str;
  while (sex.includes(find)) {
    sex = sex.replace(find, to);
  }
  return sex;
}