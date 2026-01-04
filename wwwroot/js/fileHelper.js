// File Helper for Blazor WebAssembly
// Handles file downloads and clipboard operations

window.fileHelper = {
    // Download a file from base64 content
    downloadFile: function (fileName, contentBase64, mimeType) {
        const link = document.createElement('a');
        link.href = `data:${mimeType};base64,${contentBase64}`;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },

    // Download a file from text content
    downloadTextFile: function (fileName, content) {
        const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    },

    // Download multiple files as a ZIP archive
    // files: array of { name: string, content: string }
    // zipFileName: name of the ZIP file to download
    downloadAsZip: async function (files, zipFileName) {
        try {
            // Check if JSZip is available
            if (typeof JSZip === 'undefined') {
                return { success: false, error: 'JSZip library not loaded' };
            }

            const zip = new JSZip();

            // Add each file to the ZIP
            for (const file of files) {
                zip.file(file.name, file.content);
            }

            // Generate the ZIP file
            const zipBlob = await zip.generateAsync({ 
                type: 'blob',
                compression: 'DEFLATE',
                compressionOptions: { level: 6 }
            });

            // Download the ZIP
            const url = URL.createObjectURL(zipBlob);
            const link = document.createElement('a');
            link.href = url;
            link.download = zipFileName || 'converted_files.zip';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);

            return { success: true, savedCount: files.length };
        } catch (err) {
            console.error('Error creating ZIP:', err);
            return { success: false, error: err.message };
        }
    },

    // Copy text to clipboard
    copyToClipboard: async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            console.error('Failed to copy to clipboard:', err);
            return false;
        }
    },

    // Read file as text
    readFileAsText: function (inputElement) {
        return new Promise((resolve, reject) => {
            const file = inputElement.files[0];
            if (!file) {
                reject('No file selected');
                return;
            }
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = () => reject(reader.error);
            reader.readAsText(file);
        });
    },

    // Get file info
    getFileInfo: function (inputElement) {
        const files = inputElement.files;
        const result = [];
        for (let i = 0; i < files.length; i++) {
            result.push({
                name: files[i].name,
                size: files[i].size,
                type: files[i].type,
                lastModified: files[i].lastModified
            });
        }
        return result;
    },

    // Read all files from a folder input
    readFolderFiles: async function (inputId, maxFileSize) {
        const input = document.getElementById(inputId);
        if (!input || !input.files) return [];

        const files = input.files;
        const result = [];

        for (let i = 0; i < files.length; i++) {
            const file = files[i];

            // Skip files that are too large
            if (file.size > maxFileSize) continue;

            // Skip hidden files and directories (common patterns)
            if (file.name.startsWith('.')) continue;

            try {
                const content = await this.readFileContent(file);
                result.push({
                    name: file.name,
                    size: file.size,
                    content: content,
                    relativePath: file.webkitRelativePath || file.name
                });
            } catch (err) {
                console.error(`Error reading file ${file.name}:`, err);
            }
        }

        return result;
    },

    // Read a single file's content as text
    readFileContent: function (file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = () => reject(reader.error);
            reader.readAsText(file);
        });
    }
};
