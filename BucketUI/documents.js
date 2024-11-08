import { BASE_URL } from './config.js';

document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = 'index.html';
        return;
    }

    fetchDocuments();
});

async function fetchDocuments() {
    const token = localStorage.getItem('token');
    try {
        const response = await fetch(`${BASE_URL}/ListDocumentsperUserDept`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const documents = await response.json();
            displayDocuments(documents);
        } else if (response.status === 401) {
            // Token expired or invalid
            localStorage.removeItem('token');
            window.location.href = 'index.html';
        }
    } catch (error) {
        console.error('Error fetching documents:', error);
    }
}

async function getPresignedUrl(key) {
    const token = localStorage.getItem('token');
    
    try {
        const response = await fetch(`${BASE_URL}/Get Presigned Url/`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ keyvalue: key })
        });

        if (response.ok) {
            const data = await response.json();
            // Open the URL in a new tab
            window.open(data.url, '_blank');
        } else if (response.status === 401) {
            localStorage.removeItem('token');
            window.location.href = 'index.html';
        } else {
            console.error('Error getting presigned URL');
        }
    } catch (error) {
        console.error('Error:', error);
    }
}

async function downloadFile(key) {
    const token = localStorage.getItem('token');
    try {
        const response = await fetch(`${BASE_URL}/Download Files`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ keyvalue: key })
        });

        if (response.ok) {
            // Get the blob from the response
            const blob = await response.blob();
            // Create a temporary URL for the blob
            const url = window.URL.createObjectURL(blob);
            // Create a temporary link element
            const a = document.createElement('a');
            a.href = url;
            a.download = key.split('/').pop(); // Use the last part of the key as filename
            // Append to body, click, and remove
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            a.remove();
        } else if (response.status === 401) {
            localStorage.removeItem('token');
            window.location.href = 'index.html';
        } else {
            console.error('Error downloading file');
        }
    } catch (error) {
        console.error('Error:', error);
    }
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

async function deleteDocument(key) {
    // Show confirmation popup
    if (!confirm('Are you sure you want to delete this document?')) {
        return;
    }

    const token = localStorage.getItem('token');
    try {
        const response = await fetch(`${BASE_URL}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ keyvalue: key })
        });

        if (response.ok) {
            // Refresh the documents list after successful deletion
            fetchDocuments();
        } else if (response.status === 401) {
            localStorage.removeItem('token');
            window.location.href = 'index.html';
        } else {
            console.error('Error deleting document');
        }
    } catch (error) {
        console.error('Error:', error);
    }
}

function displayDocuments(documents) {
    const documentsList = document.getElementById('documentsList');
    documentsList.innerHTML = documents.map(doc => `
        <div class="border p-4 rounded-md hover:bg-gray-50">
            <div class="flex justify-between items-start">
                <div class="flex-1">
                    <div class="flex items-center">
                        <h3 class="font-semibold text-lg">${doc.title || 'Untitled'}</h3>
                        <button onclick="getPresignedUrl('${doc.key}')" 
                                class="ml-3 text-blue-500 hover:text-blue-700 transition-colors"
                                title="Preview document">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button onclick="downloadFile('${doc.key}')" 
                                class="ml-3 text-blue-500 hover:text-blue-700 transition-colors"
                                title="Download document">
                            <i class="fas fa-download"></i>
                        </button>
                        <button onclick="deleteDocument('${doc.key}')" 
                                class="ml-3 text-red-500 hover:text-red-700 transition-colors"
                                title="Delete document">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                    <p class="text-sm text-gray-600 mt-1">${doc.description || 'No description'}</p>
                </div>
                <div class="text-right text-sm text-gray-500">
                    <p>Size: ${formatFileSize(doc.filesize)}</p>
                    <p class="mt-1">Modified: ${formatDate(doc.lastmodified)}</p>
                </div>
            </div>
        </div>
    `).join('');
}

function logout() {
    localStorage.removeItem('token');
    window.location.href = 'index.html';
}

// Add these new functions for file upload
async function handleFileSelect(event) {
    const file = event.target.files[0];
    if (!file) return;

    try {
        // First, get the presigned URL
        const presignedData = await getUploadPresignedUrl(file.name, file.type);
        
        // Then upload the file using the presigned URL
        await uploadFileToPresignedUrl(presignedData.url, file);
        
        // Refresh the documents list
        await fetchDocuments();
        
        // Clear the file input
        event.target.value = '';
    } catch (error) {
        console.error('Error uploading file:', error);
        alert('Failed to upload file. Please try again.');
    }
}

async function getUploadPresignedUrl(filename, contentType) {
    const token = localStorage.getItem('token');
    
    const response = await fetch(`${BASE_URL}/File Upload/Presigned`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            filename: filename,
            contentType: contentType
        })
    });

    if (response.ok) {
        return await response.json();

    } else if (response.status === 401) {
        localStorage.removeItem('token');
        window.location.href = 'index.html';
        throw new Error('Unauthorized');
    } else {
        throw new Error('Failed to get presigned URL');
    }

}

async function uploadFileToPresignedUrl(presignedUrl, file) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open("PUT", presignedUrl, true);
        xhr.setRequestHeader("Content-Type", file.type);
        xhr.setRequestHeader("x-amz-meta-file-name", file.name);
  
        xhr.upload.onprogress = (event) => {
          if (event.lengthComputable) {
            const percentComplete = (event.loaded / event.total) * 100;
            // Remove references to undefined progressBar and progressText
            console.log(`Upload progress: ${percentComplete.toFixed(2)}%`);
          }
        };
  
        xhr.onload = () => {
          if (xhr.status >= 200 && xhr.status < 300) {
            resolve();
          } else {
            reject(new Error(`Upload failed with status ${xhr.status}`));
          }
        };
  
        xhr.onerror = () => reject(new Error("Network error occurred"));
  
        xhr.send(file);
    });
}

// Add loading state indicators
function setUploadButtonLoading(isLoading) {
    const uploadButton = document.querySelector('button[onclick="document.getElementById(\'fileInput\').click()"]');
    if (isLoading) {
        uploadButton.disabled = true;
        uploadButton.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Uploading...';
    } else {
        uploadButton.disabled = false;
        uploadButton.innerHTML = '<i class="fas fa-upload mr-2"></i>Upload Document';
    }
}

// Update handleFileSelect to show loading state
async function handleFileSelect(event) {
    const file = event.target.files[0];
    if (!file) return;

    setUploadButtonLoading(true);
    try {
        const presignedData = await getUploadPresignedUrl(file.name, file.type);
        await uploadFileToPresignedUrl(presignedData.url, file);
        await fetchDocuments();
        event.target.value = '';
    } catch (error) {
        console.error('Error uploading file:', error);
        alert('Failed to upload file. Please try again.');
    } finally {
        setUploadButtonLoading(false);
    }
} 