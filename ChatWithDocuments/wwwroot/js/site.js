var selectedFile = "pdfId";

$(document).ready(function () { 
    ConversationItemClickedEvent();
    $('#sendBtn').click(function () {

        var selectedModel = $('#modelSelect').val();


        $('#sendBtn').prop('disabled', true);
        var userInput = $('#userInput').val().trim();
        if (userInput) {
            $('#userInput').val('');
            appendMessage('user', userInput);
            appendMessage('bot', 'Cevap hazırlanıyor...'); // Display "Cevap hazırlanıyor" message
          
            getAnswer(selectedFile, userInput,selectedModel)
                .then(responseData => {
                    // Use the response data
                    console.log(responseData);
                    replaceBotMessage(responseData); 

                    console.log('Response:', responseData);
                })
                .catch(error => {
                    replaceBotMessage(responseData);  
                    console.error('Error:', error);
                }) 
                .finally(() => { 
                $('#sendBtn').prop('disabled', false);
                });
        }
    });

    $('#userInput').keypress(function (event) {
        if (event.which == 13) {
            $('#sendBtn').click();
        }
    });

    $('#toggleDarkMode').click(function () {
        $('body').toggleClass('dark-mode');
    });
});

function appendMessage(sender, message) {
    var messageHtml = '<div class="message ' + sender + '"><div class="content">' + message + '</div></div>';
    $('.chat-history').append(messageHtml);
    $('.chat-box').scrollTop($('.chat-box')[0].scrollHeight);
}
function replaceBotMessage(message) {
    var botMessage = $('.message.bot .content').last();
    botMessage.text(message);
}

 
document.getElementById('uploded-area').addEventListener('click', function() {
    document.getElementById('file-uploader').click();
});

document.getElementById('file-uploader').addEventListener('change', function () {
    showLoadingIcon();
    var file = this.files[0];
    if (file) {
        // Disable the file uploader button
        document.getElementById('file-uploader').disabled = true;

        // Create a FormData object
        var formData = new FormData();
        formData.append('file', file);

        // Send the file to the server using fetch
        fetch('/api/File/upload', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('File upload failed.');
                }
                return response.json(); // Parse the JSON response
            })
            .then(data => {
                const success = data.success;
                const fileUrl = data.fileUrl;  
                var splitData = fileUrl.split('_');
                var guid = splitData[0];
                var filename = splitData[1];
                addConversationItem(filename, guid);
                selectedFile = guid;
                displayMessage('File uploaded successfully!', 'success');
            })
            .catch(error => {
                console.error('Error:', error);
                displayMessage('File upload failed.', 'error');
            })
            .finally(() => {
                // Enable the file uploader button
                document.getElementById('file-uploader').disabled = false;

                // Hide loading icon
                hideLoadingIcon();
            });
    }
});
function displayMessage(message, type) {
   
   /* var modalContent = document.getElementById('message-modal-content');*/
    var modalMessage = document.getElementById('modal-body-message');
    var modal = new bootstrap.Modal(document.getElementById('message-modal'));

    modalMessage.textContent = message;
 /*   modalContent.classList.add('alert-' + type);*/

    modal.show();
}
function showLoadingIcon() {
    $('.loading-overlay').removeClass('d-none');
}

function hideLoadingIcon() {
    $('.loading-overlay').addClass('d-none'); 
} 

//function addConversationItem(fileName,dbId) {
//    var conversationItemHtml = '<div class="conversation-item" dbId="'+dbId+'"><i class="fas fa-file"></i> ' + fileName + '</div>';
//    $('.conversation-list').append(conversationItemHtml);

//}
function addConversationItem(fileName, dbId) {
    var conversationItemHtml = '<div class="conversation-item" dbId="' + dbId + '"><i class="fas fa-file mr-2"></i>  <span class="file-name">' + fileName + '</span> <i class="fas fa-trash delete-icon"></i></div>';
    $('.conversation-list').append(conversationItemHtml);
    ConversationItemClickedEvent();
}

$('.conversation-list').on('click', '.delete-icon', function () {
    var conversationItem = $(this).closest('.conversation-item');
    var dbId = conversationItem.attr('dbId');
    var confirmDelete = confirm("Bu belgeyi silmek istediğinize emin misiniz?");
    if (confirmDelete) {
        deleteFile(dbId, conversationItem);
    }
});

function deleteFile(fileGuid, conversationItem) {
  
    //const requestBody = JSON.stringify({ fileGuid });
 
    fetch(`/api/File/delete/${fileGuid}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        } 
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('File delete failed.');
            }
            conversationItem.remove();
             
        })
        .then(data => {
            // Process the response data
            console.log('Response:', data);
            // Remove the conversation item from the UI
            $('.conversation-item[dbId="' + fileGuid + '"]').remove();
        })
        .catch(error => {
            // Handle any errors that occurred during the request
            console.error('Error:', error);
        });
}






function getAnswer(guid, userInput,vmodel) {
    console.log("sss "+guid);
    // Create the request body
    //const requestBody = JSON.stringify(userInput);
    const requestBody = JSON.stringify({ question: userInput, model: vmodel });

    // Return a promise that resolves with the response data
    return new Promise((resolve, reject) => {
        // Make the POST request using fetch
        fetch('/GetAnswer/'+guid, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: requestBody
        })
            .then(response => response.text())
            .then(data => {
                // Process the response data
                console.log('Response:', data);
                resolve(data);
            })
            .catch(error => {
                // Handle any errors that occurred during the request
                console.error('Error:', error);
                reject(error);
            });
    });
}
$(document).ready(function () { 
    creteFilesMenu();  
});

function creteFilesMenu() { 
    $('.conversation-list').empty();
    getFiles()
        .then(fileInfos => {
            // Add conversation-item elements for each FileInfo
            fileInfos.forEach(fileInfo => {
                console.log(fileInfo.fileName);
                addConversationItem(fileInfo.fileName, fileInfo.guid);
            });
        })
        .catch(error => {
            console.error('Error:', error);
        });
}
function getFiles() {
    // Return a promise that resolves with the FileInfo list
    return new Promise((resolve, reject) => {
        fetch('/api/File/GetUploadedFiles')
            .then(response => response.json())
            .then(fileInfos => {
                console.log(fileInfos);
                resolve(fileInfos);
            })
            .catch(error => {
                reject(error);
            });
    });
}

function ConversationItemClickedEvent() {
    $('.conversation-list').on('click', '.conversation-item', function () { 
        $('.conversation-item').removeClass('selected'); 
        $(this).addClass('selected'); 
        var dbid = $(this).attr('dbid'); 
        selectedFile = dbid;
        console.log(selectedFile);
    });
}
