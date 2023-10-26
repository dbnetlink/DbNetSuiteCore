"use strict";
class ImageViewer extends Dialog {
    constructor(id) {
        var _a, _b;
        super(id);
        this.fileName = "";
        this.$image = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("img.main-image");
        this.$download = (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("a.download-link");
        this.button("download").on("click", () => this.download());
        this.$image.on("load", () => this.openViewer());
    }
    show($img) {
        this.$image.removeAttr("width").removeAttr("height").removeAttr("style");
        this.$image.parent().removeAttr("width").removeAttr("height").removeAttr("style");
        this.$image.attr("src", $img.attr("src"));
        this.fileName = $img.data("filename");
    }
    openViewer() {
        var _a;
        this.open();
        let width = this.$image.width();
        let height = this.$image.height();
        const maxWidth = Math.trunc(this.windowWidth * 0.8);
        const maxHeight = Math.trunc(this.windowHeight * 0.8);
        if ((width > maxWidth) || (height > maxHeight)) {
            const widthExcessRatio = (width / maxWidth);
            const heightExcessRatio = (height / maxHeight);
            if (widthExcessRatio > heightExcessRatio) {
                width = Math.trunc(width / widthExcessRatio);
                height = Math.trunc(height / widthExcessRatio);
                this.$image.width(width).css("height", "auto");
            }
            else {
                height = Math.round(height / heightExcessRatio);
                width = Math.trunc(width / heightExcessRatio);
                this.$image.height(height).css("width", "auto");
            }
        }
        this.$image.parent().width(width).height(height).css('overflow', 'hidden');
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.dialog("option", "width", width + 10).dialog("option", "height", height + 110);
    }
    download() {
        const link = this.$download.get(0);
        link.href = this.$image.attr("src");
        link.download = this.fileName;
        link.click();
    }
}
