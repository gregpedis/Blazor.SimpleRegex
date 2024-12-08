function copyToClipboard(text) {
    navigator.clipboard.writeText(text)
        .then(() => {
            console.log("Text copied to clipboard: ", text);
        })
        .catch(err => {
            console.error("Could not copy text: ", err);
        });
}
