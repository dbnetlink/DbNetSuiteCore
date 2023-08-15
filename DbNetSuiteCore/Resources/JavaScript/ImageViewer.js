"use strict";
class ImageViewer extends Dialog {
    constructor(id) {
        var _a, _b;
        super(id);
        this.fileName = "";
        this.$image = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("img.main-image");
        this.$download = (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("a.download-link");
        this.button("download").on("click", () => this.download());
        this.$image.on("load", () => this.open());
    }
    show($img) {
        this.$image.attr("src", $img.attr("src"));
        this.fileName = $img.data("filename");
    }
    download() {
        const link = this.$download.get(0);
        link.href = this.$image.attr("src");
        link.download = this.fileName;
        link.click();
    }
}
