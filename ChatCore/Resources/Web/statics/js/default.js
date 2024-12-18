let userLang = "";
let language_pack;
var tts;

var channelChipsInstance;
var channelTooltipInstance;
var usernameChipsInstance;
var bilibiliBlockListUsernameTooltipInstance;
var uidChipsInstance;
var bilibiliBlockListUIDTooltipInstance;
var keywordsChipsInstance;
var bilibiliBlockListKeywordTooltipInstance;
var danmukuServiceMethodSelectorInstance;
var saveButtonElement;
var requestQRCodeElement;
var ttsSpeedRangeInstance;
var ttsPitchRangeInstance;
var qrCodeTimer;
var qrCodeCountdown;
var qrCodeImgGenerator;

// Helper functions
// https://regex101.com/r/5Nrx9S/4
var twitchChannelNameRegex = /^(?:https?:\/\/)?(?:(?:www\.|go\.)?twitch\.tv\/)?([a-z0-9_]{1,25})[\/?]?(?:.)*$/i;

function extractTwitchChannelName(input) {
	return twitchChannelNameRegex.exec(input);
}

function toggleTwitchOAuthVisibility() {
	var tokenField = document.getElementById("twitch_oauth_token");
	if (tokenField.type === "password") {
		tokenField.type = "text";
	} else {
		tokenField.type = "password";
	}
}

function toggleElement(id, force = false, show = false) {
	var element = document.getElementById(id);
	if (force) {
		if (show && element.classList.contains("hide")) {
			element.classList.remove("hide");
		} else if (!show && !element.classList.contains("hide")) {
			element.classList.add("hide");
		}
	}
	else {
		if (element.classList.contains("hide")) {
			element.classList.remove("hide");
		} else {
			element.classList.add("hide");
		}
	}
}

function toggleTTSSettings() {
	toggleElement("overlay-tts-voice-package-selector-warp");
	toggleElement("overlay-tts-voice-settings-warp");
	toggleElement("global-settings-web-streaming-overlay-tts-test-button");
}

function saveSettings(e) {
	e.preventDefault();

	saveButtonElement.enabled = false;
	saveButtonElement.classList.add("disabled")

	for (var key in data) {
		var element = document.getElementById(key);

		if (element === null) {
			continue;
		}

		if (element.tagName.toLowerCase() === "input") {
			switch (element.type) {
				case "checkbox":
					data[key] = element.checked;
					break;
				case "password":
				case "text":
					data[key] = element.value;
					break;
				case "number":
					data[key] = parseInt(element.value);
					if (isNaN(data[key])) {
						data[key] = 0;
					}
					break;
			}
		}
	}

	data.twitch_channels = channelChipsInstance.getData().map(function (channelTag) {
		return channelTag.id;
	});

	data.bilibili_block_list_username = usernameChipsInstance.getData().map(function (usernameTag) {
		return usernameTag.id;
	});

	data.bilibili_block_list_uid = uidChipsInstance.getData().map(function (uidTag) {
		return uidTag.id;
	});

	data.bilibili_block_list_keyword = keywordsChipsInstance.getData().map(function (keywordTag) {
		return keywordTag.id;
	});

	let new_data = data;
	new_data.bilibili_block_list_username = JSON.stringify(data.bilibili_block_list_username);
	new_data.bilibili_block_list_uid = JSON.stringify(data.bilibili_block_list_uid);
	new_data.bilibili_block_list_keyword = JSON.stringify(data.bilibili_block_list_keyword);
	new_data.overlay_tts_voice_package = ttsVoicePackageSelectorInstance == null ? "" : ttsVoicePackageSelectorInstance.getSelectedValues()[0];
	new_data.overlay_tts_voice_speed = parseInt(data.overlay_tts_voice_speed);
	new_data.overlay_tts_voice_pitch = parseInt(data.overlay_tts_voice_pitch);


	// new HttpRequest instance
	var request = new XMLHttpRequest();
	request.open("POST", "/submit");
	request.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
	request.onabort = function () {
		M.toast({ text: set_translation("request-onabort", false) === "" ? "Something went wrong, try again later." : set_translation("request-onabort", false) })
	};
	request.onerror = function () {
		M.toast({ text: set_translation("request-onerror", false) === "" ? "Something went wrong while trying to save the settings. Make sure Beat Saber is still running." : set_translation("request-onerror", false) })
	};
	request.onload = function () {
		if (request.readyState === 4 && request.status === 204) {
			M.toast({ text: set_translation("request-onload", false) === "" ? "Settings have been saved successfully." : set_translation("request-onload", false) })
		} else {
			M.toast({ text: set_translation("request-onerror", false) === "" ? "Something went wrong while trying to save the settings. Make sure Beat Saber is still running." : set_translation("request-onerror", false) })
		}
	};
	request.onloadend = function () {
		saveButtonElement.classList.remove("disabled")
		saveButtonElement.enabled = true;
	};
	request.send(JSON.stringify(new_data));
}

function detect_language() {
	/*console.log("Detect Language");*/
	if (!["en", "zh", "ja"].includes(userLang)) {
		userLang = navigator.language || navigator.userLanguage;
		switch (userLang) {
			case "zh":
			case "zh-Hans":
			case "zh-CN":
				userLang = "zh";
				break;
			case "ja":
			case "ja-jp":
				userLang = "ja";
				break;
			case "en":
			default:
				userLang = "en";
		}
	}

	fetch_language_file();
}

function change_language(lang) {
	userLang = lang;
	document.documentElement.lang = userLang;
	fetch_language_file();
}

function fetch_language_file() {
	data["Language"] = userLang;
	/*console.log("Fetch Language");*/
	var language_file_request = new XMLHttpRequest();
	language_file_request.open('GET', `/Statics/Lang/${userLang}.json`, true);
	language_file_request.send(null);
	language_file_request.onreadystatechange = function () {
		if (language_file_request.readyState === 4 && language_file_request.status === 200) {
			language_pack = JSON.parse(language_file_request.responseText);
			if (language_pack !== undefined && (Object.keys(language_pack)).length > 0) {
				let translation_list = [
					"title",
					"brand-logo",
					"version",
					"twitch-settings-title",
					"twitch-settings-login",
					"twitch-settings-login-button",
					"twitch-settings-oauth-token",
					"twitch-settings-oauth-token-toggle",
					"twitch-settings-channel-title",
					"twitch-settings-emote-settings",
					"twitch-settings-parse-bbtv-emotes",
					"twitch-settings-parse-ffz-emotes",
					"twitch-settings-parse-twitch-emotes",
					"twitch-settings-parse-cheermotes",
					"global-settings-title",
					"global-settings-web-app",
					"global-settings-launch-web-app-on-startup",
					"global-settings-global-setting",
					"global-settings-parse-emojis",
					"global-settings-chat-settings",
					"global-settings-chat-twitch-enable",
					"global-settings-chat-bilibili-enable",
					"save-button-text",
					"bilibili-settings-title",
					"danmuku-service-method",
					"danmuku-service-method-legacy",
					"danmuku-service-method-default",
					"danmuku-service-method-openblive",
					"bilibili-settings-room-id",
					"bilibili-settings-cookies-settings",
					"bilibili-settings-room-cookies",
					"bilibili-settings-cookies-settings-warning",
					"bilibili-settings-cookies-qr-login-status-step-text",
					"bilibili-settings-cookies-qr-login-status-request-text",
					"bilibili-settings-cookies-qr-login-status-scan-text",
					"bilibili-settings-cookies-qr-login-status-confirm-text",
					"bilibili-settings-cookies-qr-login-request-button-text",
					"bilibili-settings-blive-settings",
					"bilibili-settings-blive-settings-code",
					"bilibili-settings-danmuku-settings",
					"bilibili-settings-danmuku-settings-basic",
					"bilibili-settings-danmuku-settings-gift",
					"bilibili-settings-danmuku-settings-interaction",
					"bilibili-settings-danmuku-settings-guard",
					"bilibili-settings-danmuku-settings-notification",
					"bilibili-settings-danmuku-settings-block-list",
					"bilibili-settings-danmuku-danmuku",
					"bilibili-settings-danmuku-superchat",
					"bilibili-settings-danmuku-avatar",
					"bilibili-settings-danmuku-badge-prefix",
					"bilibili-settings-danmuku-badge-prefix-text",
					"bilibili-settings-danmuku-badge-prefix-icon",
					"bilibili-settings-danmuku-honor-badge-prefix",
					"bilibili-settings-danmuku-honor-badge-prefix-text",
					"bilibili-settings-danmuku-honor-badge-prefix-icon",
					"bilibili-settings-danmuku-broadcaster-prefix",
					"bilibili-settings-danmuku-broadcaster-prefix-text",
					"bilibili-settings-danmuku-broadcaster-prefix-icon",
					"bilibili-settings-danmuku-moderator-prefix",
					"bilibili-settings-danmuku-moderator-prefix-text",
					"bilibili-settings-danmuku-moderator-prefix-icon",
					"bilibili-settings-danmuku-gift",
					"bilibili-settings-danmuku-gift_combo",
					"bilibili-settings-danmuku-gift-combine",
					"bilibili-settings-danmuku-gift-star",
					"bilibili-settings-danmuku-interaction-enter",
					"bilibili-settings-danmuku-interaction-follow",
					"bilibili-settings-danmuku-interaction-share",
					"bilibili-settings-danmuku-interaction-special-follow",
					"bilibili-settings-danmuku-interaction-mutual-follow",
					"bilibili-settings-danmuku-interaction-guard-enter",
					"bilibili-settings-danmuku-interaction-effect",
					"bilibili-settings-danmuku-interaction-anchor",
					"bilibili-settings-danmuku-interaction-raffle",
					"bilibili-settings-danmuku-interaction-red-packet",
					"bilibili-settings-danmuku-new-guard",
					"bilibili-settings-danmuku-new-guard-msg",
					"bilibili-settings-danmuku-guard-msg",
					"bilibili-settings-danmuku-guard-lottery",
					"bilibili-settings-danmuku-guard-prefix",
					"bilibili-settings-danmuku-guard-prefix-text",
					"bilibili-settings-danmuku-guard-prefix-icon",
					"bilibili-settings-danmuku-notification-block_list",
					"bilibili-settings-danmuku-notification-room-info-change",
					"bilibili-settings-danmuku-notification-room-prepare",
					"bilibili-settings-danmuku-notification-room-online",
					"bilibili-settings-danmuku-notification-room-rank",
					"bilibili-settings-danmuku-notification-like",
					"bilibili-settings-danmuku-notification-boardcast",
					"bilibili-settings-danmuku-notification-PK",
					"bilibili-settings-danmuku-notification-junk",
					"bilibili-settings-block-list-username",
					"bilibili-settings-block-list-uid",
					"bilibili-settings-block-list-keyword",
					"bilibili-settings-utilities",
					"bilibili-settings-utilities-clean-cache",
					"bilibili-settings-utilities-images-button-text",
					"global-settings-web-streaming-overlay-settings",
					"global-settings-web-streaming-overlay-customize",
					"global-settings-web-streaming-overlay-show-init-welcome",
					"global-settings-web-streaming-overlay-show-username",
					"global-settings-web-streaming-overlay-show-gift-in-sc",
					"global-settings-web-streaming-overlay-show-guard-in-sc",
					"global-settings-web-streaming-overlay-tts",
					"global-settings-web-streaming-overlay-tts-notice",
					"global-settings-web-streaming-overlay-tts-enable",
					"global-settings-web-streaming-overlay-tts-voice-package",
					"global-settings-web-streaming-overlay-tts-voice-speed",
					"global-settings-web-streaming-overlay-tts-voice-pitch",
					"global-settings-web-streaming-overlay-tts-test-button-text",
					"global-settings-web-streaming-overlay-button-text"
				];
				translation_list.forEach((key) => {
					set_translation(key);
				});

				M.Collapsible.init(document.querySelectorAll('.collapsible'));
				M.Dropdown.init(document.querySelectorAll('.dropdown-trigger'));


				danmukuServiceMethodSelectorInstance = M.FormSelect.init(document.querySelector('#danmuku-service-method-selector'), {
				});

				danmukuServiceMethodSelectorInstance.el.addEventListener("change", function () {
					data["danmuku_service_method"] = danmukuServiceMethodSelectorInstance.getSelectedValues()[0];
					bilibili_danmuku_functions_disabled(danmukuServiceMethodSelectorInstance.getSelectedValues()[0] === "OpenBLive");
					toggle_bilibili_cookies();
				});

				toggle_bilibili_cookies();

				TTSService();

				channelTooltipInstance = M.Tooltip.init(document.querySelector('#twitch-settings-channel-tooltipped'), {
					text: set_translation("channel-tooltip", false) === "" ? "Make sure to add your channelname here! When you have filled it in, make sure to hit enter and the save button afterwards." : set_translation("channel-tooltip", false)
				});

				bilibiliBlockListUsernameTooltipInstance = M.Tooltip.init(document.querySelector('#bilibili-settings-block-list-username-tooltipped'), {
					text: set_translation("bilibili-settings-block-list-username-tooltips", false) === "" ? "The user contains the username keywords will be blocked. When you have filled it in, make sure to hit enter and the save button afterwards." : set_translation("bilibili-settings-block-list-username-tooltips", false)
				});

				bilibiliBlockListUIDTooltipInstance = M.Tooltip.init(document.querySelector('#bilibili-settings-block-list-uid-tooltipped'), {
					text: set_translation("bilibili-settings-block-list-uid-tooltips", false) === "" ? "The user with certain UIDwill be blocked. When you have filled it in, make sure to hit enter and the save button afterwards." : set_translation("bilibili-settings-block-list-uid-tooltips", false)
				});

				bilibiliBlockListKeywordTooltipInstance = M.Tooltip.init(document.querySelector('#bilibili-settings-block-list-keyword-tooltipped'), {
					text: set_translation("bilibili-settings-block-list-keyword-tooltips", false) === "" ? "The message contains the keywords will be blocked. When you have filled it in, make sure to hit enter and the save button afterwards." : set_translation("bilibili-settings-block-list-keyword-tooltips", false)
				});

				channelChipsInstance = M.Chips.init(document.querySelector('#twitch-settings-channels'), {
					data: data.twitch_channels
						.filter(function (channelName) {
							return channelName.length > 0
						})
						.map(function (channelName) {
							return { id: channelName }
						}),
					secondaryPlaceholder: set_translation("channel-chip-secondary", false) === "" ? "Add channel" : set_translation("channel-chip-secondary", false),
					limit: 10,
					onChipAdd: function (container, newlyAddedChannelChip) {
						var rawChannelNameTagEntry = decodeURIComponent(newlyAddedChannelChip.firstChild.data);
						var channelName = extractTwitchChannelName(rawChannelNameTagEntry);
						var channelTagIndex = Array.prototype.slice.call(container.children).indexOf(newlyAddedChannelChip);

						if (!channelName) {
							channelChipsInstance.deleteChip(channelTagIndex);
							M.toast({ text: set_translation("channel-chip-delete-invalid", false) === "" ? 'Tried to add an invalid channelname' : set_translation("channel-chip-delete-invalid", false) })
							return;
						} else if (channelName === "REDRAWCHIP") {
							channelChipsInstance.deleteChip(uidChipTagIndex);
							return;
						} else if (rawChannelNameTagEntry !== channelName[1]) {
							channelChipsInstance.deleteChip(channelTagIndex);
							channelChipsInstance.addChip({ tag: channelName[1] });
							return;
						}

						M.toast({
							text: (set_translation("channel-chip-add-1", false) === "" ? "Added channel " : set_translation("channel-chip-add-1", false)) +
								channelName[1] +
								(set_translation("channel-chip-add-2", false) === "" ? ", don't forget to save." : set_translation("channel-chip-add-2", false))
						});
					}
				});

				usernameChipsInstance = M.Chips.init(document.querySelector('#bilibili-settings-block-list-usernames'), {
					data: data.bilibili_block_list_username
						.filter(function (username) {
							return username.toString().length > 0
						})
						.map(function (username) {
							return { id: username }
						}),
					secondaryPlaceholder: set_translation("bilibili-settings-block-list-username-chip-secondary", false) === "" ? "Add username" : set_translation("bilibili-settings-block-list-username-chip-secondary", false),
					onChipAdd: function (container, newlyAddedUsernameChip) {
						var newUsername = newlyAddedUsernameChip.firstChild.data.trim();
						var usernameChipTagIndex = Array.prototype.slice.call(container.children).indexOf(newlyAddedUsernameChip);

						if (newUsername === "") {
							usernameChipsInstance.deleteChip(usernameChipTagIndex);
							M.toast({ text: set_translation("bilibili-settings-block-list-username-chip-delete-invalid", false) === "" ? 'Tried to add an invalid username' : set_translation("bilibili-settings-block-list-username-chip-delete-invalid", false) })
							return;
						} else if (newUsername === "REDRAWCHIP") {
							usernameChipsInstance.deleteChip(uidChipTagIndex);
							return;
						} else if (newUsername !== newlyAddedUsernameChip.firstChild.data) {
							usernameChipsInstance.deleteChip(usernameChipTagIndex);
							usernameChipsInstance.addChip({ tag: newUsername });
							newlyAddedUsernameChip = '';
							return;
						}

						M.toast({
							text: (set_translation("bilibili-settings-block-list-username-chip-add-1", false) === "" ? "Added Username " : set_translation("bilibili-settings-block-list-username-chip-add-1", false)) +
								newUsername +
								(set_translation("bilibili-settings-block-list-username-chip-add-2", false) === "" ? ", don't forget to save." : set_translation("bilibili-settings-block-list-username-chip-add-2", false))
						});
					}
				});

				uidChipsInstance = M.Chips.init(document.querySelector('#bilibili-settings-block-list-uids'), {
					data: data.bilibili_block_list_uid
						.filter(function (uid) {
							return uid > 0
						})
						.map(function (uid) {
							return { id: uid }
						}),
					secondaryPlaceholder: set_translation("bilibili-settings-block-list-uid-chip-secondary", false) === "" ? "Add UID" : set_translation("bilibili-settings-block-list-uid-chip-secondary", false),
					onChipAdd: function (container, newlyAddedUIDChip) {
						var newUID = newlyAddedUIDChip.firstChild.data;
						var uidChipTagIndex = Array.prototype.slice.call(container.children).indexOf(newlyAddedUIDChip);

						if (newUID === "REDRAWCHIP") {
							uidChipsInstance.deleteChip(uidChipTagIndex);
							return;
						} else if (isNaN(parseInt(newUID)) || parseInt(newUID) < 0) {
							uidChipsInstance.deleteChip(uidChipTagIndex);
							M.toast({ text: set_translation("bilibili-settings-block-list-uid-chip-delete-invalid", false) === "" ? 'Tried to add an invalid UID' : set_translation("bilibili-settings-block-list-uid-chip-delete-invalid", false) })
							return;
						}

						M.toast({
							text: (set_translation("bilibili-settings-block-list-uid-chip-add-1", false) === "" ? "Added UID " : set_translation("bilibili-settings-block-list-uid-chip-add-1", false)) +
								newUID +
								(set_translation("bilibili-settings-block-list-uid-chip-add-2", false) === "" ? ", don't forget to save." : set_translation("bilibili-settings-block-list-uid-chip-add-2", false))
						});
					}
				});

				keywordsChipsInstance = M.Chips.init(document.querySelector('#bilibili-settings-block-list-keywords'), {
					data: data.bilibili_block_list_keyword
						.filter(function (keywords) {
							return keywords.toString().length > 0
						})
						.map(function (keywords) {
							return { id: keywords }
						}),
					secondaryPlaceholder: set_translation("bilibili-settings-block-list-keyword-chip-secondary", false) === "" ? "Add keyword" : set_translation("bilibili-settings-block-list-keyword-chip-secondary", false),
					onChipAdd: function (container, newlyAddedKeywordChip) {
						var newKeyword = newlyAddedKeywordChip.firstChild.data.trim();
						var keywordChipTagIndex = Array.prototype.slice.call(container.children).indexOf(newlyAddedKeywordChip);

						if (newKeyword === "REDRAWCHIP") {
							keywordsChipsInstance.deleteChip(keywordChipTagIndex);
							return;
						} else if (newKeyword === "") {
							keywordsChipsInstance.deleteChip(keywordChipTagIndex);
							M.toast({ text: set_translation("bilibili-settings-block-list-keyword-chip-delete-invalid", false) === "" ? 'Tried to add an invalid keyword' : set_translation("bilibili-settings-block-list-keyword-chip-delete-invalid", false) })
							return;
						} else if (newKeyword !== newlyAddedKeywordChip.firstChild.data) {
							keywordsChipsInstance.deleteChip(keywordChipTagIndex);
							newlyAddedKeywordChip = newKeyword;
							keywordsChipsInstance.addChip({ tag: newKeyword });
							newlyAddedKeywordChip = '';
							return;
						}

						M.toast({
							text: (set_translation("bilibili-settings-block-list-keyword-chip-add-1", false) === "" ? "Added Keyword " : set_translation("bilibili-settings-block-list-keyword-chip-add-1", false)) +
								newKeyword +
								(set_translation("bilibili-settings-block-list-keyword-chip-add-2", false) === "" ? ", don't forget to save." : set_translation("bilibili-settings-block-list-keyword-chip-add-2", false))
						});
					}
				});

				function redrawChipInstance(el) {
					if (el.getData().length === 0) {
						el.addChip({ id: "REDRAWCHIP" });
					}
				}

				[channelChipsInstance, usernameChipsInstance, uidChipsInstance, keywordsChipsInstance].forEach((el) => redrawChipInstance(el));

				function toggle_bilibili_cookies() {
					if (!bilibili_version && danmukuServiceMethodSelectorInstance.getSelectedValues()[0] === "Default") {
						if ($("#bilibili-settings-cookies-div").hasClass("hide")) {
							$("#bilibili-settings-cookies-div").removeClass("hide");
						}
					} else {
						if (!$("#bilibili-settings-cookies-div").hasClass("hide")) {
							$("#bilibili-settings-cookies-div").addClass("hide");
						}
					}
				}
			}
		}
	}
}

function set_translation(id, dom = true) {
	// console.log(`Set ${id}, is DOM? ${dom ? "true" : "false"}, text: ${language_pack[id]}`);
	if (language_pack.hasOwnProperty(id)) {
		if (dom) {
			if (id === "danmuku-service-method" && $(`#${id}`).length === 0) {
				danmukuServiceMethodSelectorInstance?.destroy();
				$('#danmuku-service-method-selector').after($("<label></label>").attr({ id: 'danmuku-service-method', for: 'danmuku-service-method-selector' }).text(language_pack[id]));
			}
			else if (id === "global-settings-web-streaming-overlay-tts-voice-package" && $(`#${id}`).length === 0) {
				ttsVoicePackageSelectorInstance?.destroy();
				$('#overlay-tts-voice-package-selector').after($("<label></label>").attr({ id: 'global-settings-web-streaming-overlay-tts-voice-package', for: 'overlay-tts-voice-package-selector' }).text(language_pack[id]));
			}
			else if (id === "title") {
				document.title = language_pack[id] === "" ? document.title : language_pack[id];
			} else {
				$(`#${id}`).text(language_pack[id] === "" ? $(`#${id}`).text() : language_pack[id]);
			}
		} else {
			return language_pack[id];
		}
	}

	return "";
}

function bilibili_danmuku_functions_disabled(status) {
	let disabled_function_list = [
		"danmuku_honor_badge_prefix",
		"danmuku_honor_badge_prefix_type",
		"danmuku_badge_prefix_type",
		"danmuku_broadcaster_prefix",
		"danmuku_broadcaster_prefix_type",
		"danmuku_moderator_prefix",
		"danmuku_moderator_prefix_type",
		"danmuku_gift_combo",
		"danmuku_interaction_enter",
		"danmuku_interaction_follow",
		"danmuku_interaction_share",
		"danmuku_interaction_special_follow",
		"danmuku_interaction_mutual_follow",
		"danmuku_interaction_guard_enter",
		"danmuku_interaction_effect",
		"danmuku_interaction_anchor",
		"danmuku_interaction_raffle",
		"danmuku_interaction_red_packet",
		"danmuku_new_guard",
		"danmuku_guard_msg",
		"danmuku_guard_lottery",
		"danmuku_notification_block_list",
		"danmuku_notification_room_info_change",
		"danmuku_notification_room_prepare",
		"danmuku_notification_room_online",
		"danmuku_notification_room_rank",
		"danmuku_notification_boardcast",
		"danmuku_notification_pk"
		
	];
	disabled_function_list.forEach((id) => {
		$(`#${id}`).prop("disabled", status)
	});
}

$(function () {
	for (var key in data) {
		var value = data[key];
		var element = document.getElementById(key);
		if (element == null) {
			if (["bilibili_block_list_keyword", "bilibili_block_list_uid", "bilibili_block_list_username"].includes(key)) {
				data[key] = JSON.parse(value);
			} else if (key === "danmuku_service_method") {
				document.querySelector('#danmuku-service-method-selector').value = value;
				// $('#danmuku-service-method-selector').val("Default").change();
				// $('#danmuku-service-method-selector').val(value);
			} else if (key === "overlay_tts_voice_package" && value !== "") {
				document.querySelector('#overlay-tts-voice-package-selector').value = value;
			}
			continue;
		}
		if (typeof value === "string" || typeof value === "number") {
			if (key === "overlay_tts_voice_speed" || key === "overlay_tts_voice_pitch") {
				element.value = value / 10;
			} else {
				element.value = value;
			}
		} else if (typeof value === "boolean") {
			element.checked = value;
			if (key == "EnableTwitch" && value) {
				toggleElement("twitch-settings-card")
			}

			if (key == "EnableBilibili" && value) {
				toggleElement("bilibili-settings-card")
			}

			if (key == "overlay_tts_enable" && !value) {
				toggleTTSSettings();
			}
		}
	}

	document.getElementById("twitch_oauth_token_visible").checked = document.getElementById("twitch_oauth_token").type !== "password";

	var oauthHash = location.hash.substring(1);
	var accessToken = oauthHash.substring(oauthHash.indexOf('access_token=')).split('&')[0].split('=')[1];

	// Clean up url
	history.pushState({}, "ChatCore Settings", location.origin)

	if (accessToken) {
		var tokenField = document.getElementById("twitch_oauth_token");
		tokenField.value = 'oauth:' + decodeURIComponent(accessToken);
	}

	saveButtonElement = document.getElementById("saveButton");
	saveButtonElement.addEventListener("click", saveSettings);
	document.getElementById("BLive_submit").addEventListener("click", saveSettings);

	if (bilibili_version) {
		if (!$("#twitch-settings-switch").hasClass("hide")) {
			$("#twitch-settings-switch").addClass("hide");
		}
	}
	userLang = data["Language"];
	tts = new TTS();
	detect_language();
	document.getElementById("bilibili-settings-utilities-images-button").addEventListener("click", clean_cache_bilibili_image);
	document.getElementById("global-settings-web-streaming-overlay-tts-test-button").addEventListener("click", TTSTest);
	requestQRCodeElement = document.getElementById("bilibili-settings-cookies-qr-login-request-button");
	requestQRCodeElement.addEventListener("click", request_new_bilibili_login_qr_code);

	ttsSpeedRangeInstance = M.Range.init(document.getElementById("overlay_tts_voice_speed"));
	ttsPitchRangeInstance = M.Range.init(document.getElementById("overlay_tts_voice_pitch"));

	ttsSpeedRangeInstance.el.addEventListener("click", change_tts_voice_speed);
	ttsPitchRangeInstance.el.addEventListener("click", change_tts_voice_pitch);
	// $('.collapsible').collapsible();
	// $('.dropdown-trigger').dropdown();
	//$('select').formSelect();
});

function clear_blive_code() {
	$('#bilibili_identity_code').val("");
}

function clean_cache_bilibili_image() {
	var taget_button = document.getElementById("bilibili-settings-utilities-images-button")
	
	taget_button.enabled = false;
	taget_button.classList.add("disabled");
	
	// new HttpRequest instance
	var request = new XMLHttpRequest();
	request.open("GET", "/clean/cache/images");
	request.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
	request.onabort = function () {
		M.toast({ text: set_translation("request-onabort", false) === "" ? "Something went wrong, try again later." : set_translation("request-onabort", false) })
	};
	request.onerror = function () {
		M.toast({ text: set_translation("request-onerror-clean-cache-bilibili-image", false) === "" ? "Something went wrong while trying to clean image cache. Make sure Beat Saber is still running." : set_translation("request-onerror-clean-cache-bilibili-image", false) })
	};
	request.onload = function () {
		if (request.readyState === 4 && request.status === 200) {
			M.toast({ text: set_translation("request-onload-clean-cache-bilibili-image", false) === "" ? "Image cache has been cleaned successfully." : set_translation("request-onload-clean-cache-bilibili-image", false) })
		} else {
			M.toast({ text: set_translation("request-onerror-clean-cache-bilibili-image", false) === "" ? "Something went wrong while trying to clean image cache. Make sure Beat Saber is still running." : set_translation("request-onerror-clean-cache-bilibili-image", false) })
		}
	};
	request.onloadend = function () {
		taget_button.classList.remove("disabled")
		taget_button.enabled = true;
	};
	request.send();
}

function TTSService() {
	let voiceSelect = document.getElementById('overlay-tts-voice-package-selector');
	tts.voices.then(voices => {
		voiceSelect.innerHTML = "";
		for (let i = 0; i < voices.length; i++) {
			if (voices[i].lang.includes(userLang)) {
				if (data["overlay_tts_voice_package"] === "") {
					document.querySelector('#overlay-tts-voice-package-selector').value = voices[i].name;
					data["overlay_tts_voice_package"] = voices[i].name;
					tts.selectVoiceByIndex(i);
				}
				let option = document.createElement('option');
				option.textContent = ((voices[i].localService ? '🖥' : '☁') + " " + voices[i].name);
				option.setAttribute('data-lang', voices[i].lang);
				option.setAttribute('data-name', voices[i].name);
				option.setAttribute('data-index', `${i}`);
				option.setAttribute('value', voices[i].name);
				voiceSelect.appendChild(option);
				if (data["overlay_tts_voice_package"] == voices[i].name) {
					document.querySelector('#overlay-tts-voice-package-selector').value = voices[i].name;
				}
			}
		}
		ttsVoicePackageSelectorInstance = M.FormSelect.init(document.querySelector('#overlay-tts-voice-package-selector'), {
		});

		ttsVoicePackageSelectorInstance.el.addEventListener("change", function () {
			tts.selectVoiceByName(ttsVoicePackageSelectorInstance.getSelectedValues()[0]);
			data["overlay_tts_voice_package"] = ttsVoicePackageSelectorInstance.getSelectedValues()[0];
		});
	});
}

function TTSTest() {
	if (document.getElementById("overlay_tts_enable").checked) {
		tts.selectVoiceByName(ttsVoicePackageSelectorInstance.getSelectedValues()[0]);
		tts.test()
	}
}

function change_tts_voice_speed() {
	data["overlay_tts_voice_speed"] = parseFloat(ttsSpeedRangeInstance.value.textContent) * 10;
	tts.rate = parseFloat(ttsSpeedRangeInstance.value.textContent);
}

function change_tts_voice_pitch() {
	data["overlay_tts_voice_pitch"] = parseFloat(ttsPitchRangeInstance.value.textContent) * 10;
	tts.pitch = parseFloat(ttsPitchRangeInstance.value.textContent);
}

function request_new_bilibili_login_qr_code() {
	reset_bilibili_login_steps();
	requestQRCodeElement.enabled = false;
	requestQRCodeElement.classList.add("disabled");
	set_bilibili_login_progress('busy');

	var request = new XMLHttpRequest();
	request.open("GET", "/bilibili_qr_request");
	request.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
	request.onabort = function () {
		set_bilibili_login_progress('error');
		$("#bilibili-settings-cookies-qr-login-status-request-icon").text("error");
		M.toast({ text: set_translation("request-onabort", false) === "" ? "Something went wrong, try again later." : set_translation("request-onabort", false) })
	};
	request.onerror = function () {
		set_bilibili_login_progress('error');
		$("#bilibili-settings-cookies-qr-login-status-request-icon").text("error");
		M.toast({ text: set_translation("request-onerror-request-qr-code", false) === "" ? "Something went wrong while trying to get QR code. Make sure Beat Saber is still running." : set_translation("request-onerror-request-qr-code", false) })
	};
	request.onload = function () {
		if (request.readyState === 4 && request.status === 200) {
			let response = JSON.parse(request.responseText);
			toggleElement("bilibili-settings-cookies-qr-img", true, true);
			if (qrCodeImgGenerator !== null && qrCodeImgGenerator !== undefined) {
				qrCodeImgGenerator.makeCode(response["url"]);
			} else {
				qrCodeImgGenerator = new QRCode("bilibili-settings-cookies-qr-img", response["url"]);
			}
			
			qrCodeTimer = setTimeout(qrCodeTimer_heartbeat, 1000);
			$("#bilibili-settings-cookies-qr-login-status-request-icon").text("done");
			M.toast({ text: set_translation("request-onload-request-qr-code", false) === "" ? "Requesting QR code successfully." : set_translation("request-onload-request-qr-code", false) })
		} else {
			set_bilibili_login_progress('error');
			$("#bilibili-settings-cookies-qr-login-status-request-icon").text("error");
			M.toast({ text: set_translation("request-onerror-request-qr-code", false) === "" ? "Something went wrong while trying to get QR code. Make sure Beat Saber is still running." : set_translation("request-onerror-request-qr-code", false) })
		}
	};
	request.onloadend = function () {
		requestQRCodeElement.classList.remove("disabled")
		requestQRCodeElement.enabled = true;
	};

	request.send();
}

function reset_bilibili_login_steps() {
	$("#bilibili-settings-cookies-qr-login-status-request-icon").text("pending");
	$("#bilibili-settings-cookies-qr-login-status-scan-icon").text("qr_code_scanner");
	$("#bilibili-settings-cookies-qr-login-status-confirm-icon").text("login");
	set_bilibili_login_progress('init');
	toggleElement("bilibili-settings-cookies-qr-img", true, false);
	qrCodeCountdown = 180;
	qrCodeTimer !== null && qrCodeTimer !== undefined && clearTimeout(qrCodeTimer);
	qrCodeImgGenerator !== null && qrCodeImgGenerator !== undefined && qrCodeImgGenerator.clear();
}

function set_bilibili_login_progress(status, progress = 0) {
	switch (status) {
		case "init":
			$("#bilibili-settings-cookies-qr-login-status-progress").removeClass().addClass('determinate green');
			$("#bilibili-settings-cookies-qr-login-status-progress").css('width', "0%");
			break;
		case "busy":
			$("#bilibili-settings-cookies-qr-login-status-progress").removeClass().addClass('indeterminate flash-color');
			$("#bilibili-settings-cookies-qr-login-status-progress").css('width', "0%");
			return;
		case "countdown":
			$("#bilibili-settings-cookies-qr-login-status-progress").removeClass().addClass('determinate blue');
			$("#bilibili-settings-cookies-qr-login-status-progress").css('width', `${progress}%`);
			break;
		case "error":
			$("#bilibili-settings-cookies-qr-login-status-progress").removeClass().addClass('determinate red');
			$("#bilibili-settings-cookies-qr-login-status-progress").css('width', "100%");
			break;
		case "done":
			$("#bilibili-settings-cookies-qr-login-status-progress").removeClass().addClass('determinate green');
			$("#bilibili-settings-cookies-qr-login-status-progress").css('width', "100%");
			break;
		default:
			return;
	}
	$("#bilibili-settings-cookies-qr-login-status-progress").empty();
}

function qrCodeTimer_heartbeat() {
	qrCodeCountdown--;
	set_bilibili_login_progress('countdown', qrCodeCountdown / 1.8);

	var request = new XMLHttpRequest();
	request.open("GET", "/bilibili_qr_status");
	request.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
	request.onabort = function () {
		set_bilibili_login_progress('error');
		$("#bilibili-settings-cookies-qr-login-status-confirm-icon").text("error");
		M.toast({ text: set_translation("request-onabort", false) === "" ? "Something went wrong, try again later." : set_translation("request-onabort", false) })
	};
	request.onerror = function () {
		set_bilibili_login_progress('error');
		$("#bilibili-settings-cookies-qr-login-status-confirm-icon").text("error");
		M.toast({ text: set_translation("request-onerror-request-qr-code-status", false) === "" ? "Something went wrong while trying to get QR code status. Make sure Beat Saber is still running." : set_translation("request-onerror-request-qr-code-status", false) })
	};
	request.onload = function () {
		if (request.readyState === 4 && request.status === 200) {
			let response = JSON.parse(request.responseText);
			switch (response['status']) {
				case "login_done":
					$("#bilibili-settings-cookies-qr-login-status-scan-icon").text("done");
					$("#bilibili-settings-cookies-qr-login-status-confirm-icon").text("done");
					set_bilibili_login_progress('done');
					$('#bilibili_cookies').val(response['cookies']);
					toggleElement("bilibili-settings-cookies-qr-img", true, false);
					return;
				case "qr_scan_done":
					$("#bilibili-settings-cookies-qr-login-status-scan-icon").text("done");
				case "qr_fetch_done":
				case "qr_scan_busy":
					qrCodeTimer = setTimeout(qrCodeTimer_heartbeat, 1000);
					break;
				default:
					console.log(response['status']);
					set_bilibili_login_progress('error');
					$("#bilibili-settings-cookies-qr-login-status-confirm-icon").text("error");
					toggleElement("bilibili-settings-cookies-qr-img", true, false);
					break;
			}
		} else {
			set_bilibili_login_progress('error');
			$("#bilibili-settings-cookies-qr-login-status-confirm-icon").text("error");
			M.toast({ text: set_translation("request-onerror-request-qr-code-status", false) === "" ? "Something went wrong while trying to get QR code status. Make sure Beat Saber is still running." : set_translation("request-onerror-request-qr-code-status", false) })
		}
	};

	request.send();
}