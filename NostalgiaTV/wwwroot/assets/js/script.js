
/* Initial Screen */
$(".btn-serie").on("click", function (btn) {
	const channelName = btn.currentTarget.getAttribute('serie');
	videoPlayer.muted = false;
	videoPlayer.volume = 1;
	setChannel(channelName);
});

$(function () {
	ChannelInURL();
});

function ChannelInURL() {
	const location = window.location;
	const urlPaths = location.pathname.split('/');

	if (urlPaths.length >= 1 && urlPaths[1] !== '') {
		const channelName = urlPaths[1].replaceAll("-", " ");
		setChannel(channelName);
	}
}