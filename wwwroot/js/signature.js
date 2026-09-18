// Simpele handtekening-pad: tekenen met muis, vinger of stylus op een <canvas>.
window.signaturePad = {
    pads: {},

    init: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        ctx.lineWidth = 2.5;
        ctx.lineCap = 'round';
        ctx.strokeStyle = '#000000';

        let drawing = false;
        let last = null;

        function getPos(e) {
            const rect = canvas.getBoundingClientRect();
            const point = e.touches && e.touches.length ? e.touches[0] : e;
            return {
                x: (point.clientX - rect.left) * (canvas.width / rect.width),
                y: (point.clientY - rect.top) * (canvas.height / rect.height)
            };
        }

        function start(e) {
            drawing = true;
            last = getPos(e);
            e.preventDefault();
        }

        function move(e) {
            if (!drawing) return;
            const pos = getPos(e);
            ctx.beginPath();
            ctx.moveTo(last.x, last.y);
            ctx.lineTo(pos.x, pos.y);
            ctx.stroke();
            last = pos;
            e.preventDefault();
        }

        function end() {
            drawing = false;
        }

        canvas.addEventListener('mousedown', start);
        canvas.addEventListener('mousemove', move);
        canvas.addEventListener('mouseup', end);
        canvas.addEventListener('mouseleave', end);
        canvas.addEventListener('touchstart', start, { passive: false });
        canvas.addEventListener('touchmove', move, { passive: false });
        canvas.addEventListener('touchend', end);

        this.pads[canvasId] = { canvas, ctx };
    },

    clear: function (canvasId) {
        const pad = this.pads[canvasId];
        if (!pad) return;
        pad.ctx.clearRect(0, 0, pad.canvas.width, pad.canvas.height);
    },

    isEmpty: function (canvasId) {
        const pad = this.pads[canvasId];
        if (!pad) return true;
        const blank = document.createElement('canvas');
        blank.width = pad.canvas.width;
        blank.height = pad.canvas.height;
        return pad.canvas.toDataURL() === blank.toDataURL();
    },

    getDataUrl: function (canvasId) {
        const pad = this.pads[canvasId];
        if (!pad) return null;
        return pad.canvas.toDataURL('image/png');
    }
};
