class VideoPlayer {
    constructor() {
        this.hub = "/channels";
        this.channel = '';
        this.player = null;
        this.episodeData = null;
        this.connection = null;
        this.timer = null;
        this.muted = false;
        this.volume = 0;
        this.timerIntervalUpdateElapsedTime = 1 // Interval in Seconds
        this.TIME_SYNC_THRESHOLD = 20;
    }

    async initialize(channelName) {
        this.channel = channelName;
        try {
            this.player = this.initVideoJS();
            await this.setupPlayer(channelName);
            this.player.muted(this.muted);
            this.player.volume(this.volume);
        } catch (error) {
            console.error('Error initializing video player:', error);
        }
    }

    initVideoJS() {
        return videojs('video', { autoplay: true, muted: true }); // Simplificado
    }

    async setupPlayer(channelName) {
        await this.initializeSignalR(channelName);
        this.setupPlayerEvents();
    }

    async initializeSignalR(channelName) {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${this.hub}?channel=${encodeURIComponent(channelName)}`)
            .build();

        this.connection.on('ReceiveEpisode', (episode) => this.handleNewEpisode(episode));
        this.connection.on("ElapsedTime", (elapsedTimes) => this.handleElapsedTime(elapsedTimes));

        try {
            await this.connection.start();
        } catch (error) {
            console.error('SignalR Connection Error:', error);
        }
    }

    async disconnectSignalR() {
        if (this.connection) {
            await this.connection.stop();
            this.connection.off('ReceiveEpisode');
            this.connection.off('ElapsedTime');
        }
    }

    async getElapsedTime() {
        try {
            await this.connection.invoke('GetElapsedTime', this.channel);
        } catch (error) {
            console.error('Error getting elapsed time:', error);
        }
    }

    async handleNewEpisode(episode) {
        const parsedEpisode = JSON.parse(episode);

        this.episodeData = {
            ...parsedEpisode,
            DatePlay: new Date(parsedEpisode.DatePlay)
        };

        const playlist = [{
            sources: [{
                src: this.episodeData.Path,
                type: 'video/mp4'
            }],
            title: this.episodeData.Title
        }];

        this.player.playlist(playlist);
        await videoPlayer.player.playlist.currentItem(0);
        this.getElapsedTime();


        let overlay_content = `
            <div id="vjs-playing-title" class="row gx-0 m-2">
                <div class="col-12 text-truncate">
                    <h3 class="mb-1">${this.episodeData.Serie} - ${this.episodeData.Title}</h3>
                </div>
                <div class="col-12">
                    <h4 class="mb-0">Temporada: ${this.episodeData.Season} Episodio: ${this.episodeData.Episode}</h4>
                </div>
            </div>
        `;

        this.player.overlay({
            overlays: [{
                start: 'playing',
                content: overlay_content,
                align: 'top',
            },
            {
                start: 'playing',
                content: '<div id="vjs-crt-effect" class="crt-effect"></div>',
                align: 'top',
            }]
        });

        // Solo actualiza playlist si el player no está reproduciendo
        if (!this.player.paused()) {
            this.player.play().catch(error => console.error('Playback error:', error));
        }
    }

    handleElapsedTime(elapsedTime) {
        const currentTime = this.player.currentTime();
        const diferenceTime = elapsedTime - currentTime;
        if (Math.abs(diferenceTime) >= this.TIME_SYNC_THRESHOLD) {
            this.player.currentTime(elapsedTime);
        }
    }

    setupPlayerEvents() {
        this.player.on('error', (error) => {
            console.error('Player error:', error);
        });
    }
}

const videoPlayer = new VideoPlayer();

function setChannel(channelName) {
    videoPlayer.initialize(channelName);
}