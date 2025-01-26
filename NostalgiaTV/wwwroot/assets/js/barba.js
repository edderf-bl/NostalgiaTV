const animationConfig = {
    duration: 1000,
    staggerDelay: 100,
    glitchDuration: 1200
};

import barba from 'https://cdn.jsdelivr.net/npm/@barba/core/+esm';
import anime from 'https://cdn.jsdelivr.net/npm/animejs@3.2.1/+esm';

const wrapper = document.createElement('div');
wrapper.classList.add('barba-wrapper');
document.body.appendChild(wrapper);


// Función de slide simplificada
const slide = (targets, step) => {
    const duration = animationConfig.duration;
    const from = step === 'leave' ? 0 : 100;
    const to = step === 'leave' ? 100 : 0;

    // Setear posición inicial
    targets.style.transform = `translateY(-${from}%)`;

    const translateY = `${to}%`;
    const staggerY = window.innerHeight * 0.1;

    const anim = anime.timeline({
        easing: 'easeInOutQuart',
        duration: duration
    });

    // Animación principal
    anim.add({
        targets,
        translateY,
    });

    // Animación de elementos internos al entrar
    if (step === 'enter') {
        anim.add({
            targets: targets.querySelectorAll('main > *'),
            translateY: [-staggerY, 0],
            duration: duration * 0.6,
            easing: 'easeOutQuart',
            delay: anime.stagger(animationConfig.staggerDelay)
        }, '-=500');
    }

    return anim.finished;
};

barba.hooks.before(() => {
    barba.wrapper.classList.add('is-animating');
});

barba.hooks.after(() => {
    barba.wrapper.classList.remove('is-animating');
});

barba.hooks.leave(async (data) => {
    if (data.current.namespace !== "home") {
        const player = videojs.getPlayer('video');
        if (player) {
            player.dispose();
            await videoPlayer.disconnectSignalR();
        }
    }

    ChannelInURL();


    $(".btn-serie").on("click", function (btn) {
        videoPlayer.muted = false;
        videoPlayer.volume = 1;
    });
});

// Inicializar Barba con una sola transición
barba.init({
    root: wrapper,
    preventRunning: true,
    transitions: [
        {
            name: 'slide-transition',
            sync: true,
            leave: ({ current }) => slide(current.container, 'leave'),
            enter: ({ next }) => slide(next.container, 'enter')
        }
    ]
});