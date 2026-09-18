// Zet een HTML-element (het werkbon-document) om naar een gedownloade PDF,
// geschaald zodat alles op precies één A4-pagina past.
window.pdfExport = {
    exportElement: async function (elementId, filename) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const canvas = await html2canvas(element, {
            scale: 2,
            useCORS: true,
            backgroundColor: '#ffffff',
            ignoreElements: (el) => el.classList && el.classList.contains('no-print')
        });

        const imgData = canvas.toDataURL('image/png');
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF('p', 'mm', 'a4');

        const marginMm = 8;
        const pageWidth = pdf.internal.pageSize.getWidth();
        const pageHeight = pdf.internal.pageSize.getHeight();
        const maxWidth = pageWidth - marginMm * 2;
        const maxHeight = pageHeight - marginMm * 2;

        const canvasRatio = canvas.width / canvas.height;
        let renderWidth = maxWidth;
        let renderHeight = renderWidth / canvasRatio;

        if (renderHeight > maxHeight) {
            renderHeight = maxHeight;
            renderWidth = renderHeight * canvasRatio;
        }

        const x = (pageWidth - renderWidth) / 2;
        const y = marginMm;

        pdf.addImage(imgData, 'PNG', x, y, renderWidth, renderHeight);
        pdf.save(filename);
    }
};
