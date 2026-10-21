window.daylight = {
    timeZone: () => Intl.DateTimeFormat().resolvedOptions().timeZone,
    openDialog: dialog => {
        const dismiss = () => dialog.querySelector("[data-dialog-close]")?.click();
        dialog.addEventListener("cancel", event => {
            event.preventDefault();
            dismiss();
        });
        dialog.addEventListener("click", event => {
            if (event.target !== dialog) return;
            const bounds = dialog.getBoundingClientRect();
            if (event.clientX < bounds.left || event.clientX > bounds.right ||
                event.clientY < bounds.top || event.clientY > bounds.bottom) dismiss();
        });
        dialog.showModal();
    },
    closeDialog: dialog => dialog.close()
};
