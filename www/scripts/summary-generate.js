let folderInput = document.querySelector('#input-folder');
let folderDatalist = document.querySelector('#datalist-folder');
let lastFolderInputValue = '';

let generateButton = document.querySelector('#form-btn-generate');
let resetButton = document.querySelector('#form-btn-reset');

resetButton.addEventListener('click', () => {
    let inputs = Array.from(document.getElementsByTagName('input') );

    inputs.forEach(input => {
        if(input.type === 'checkbox')
            input.checked = false;
        else input.value = '';
    } );
} );

generateButton.addEventListener('click' , generateSummary);
folderInput.addEventListener('focusin', onInputFolderChange);
folderInput.addEventListener('input', onInputFolderChange);

function onInputFolderChange(){
    let value = folderInput.value = folderInput.value.replaceAll('/', '\\');
    if(value.length > 0){
        if(value[0] === ' ')
            value = value.substring(1);
    }

    let updateDatalist = false;

    let suggestions = Array.from(folderDatalist.getElementsByTagName('option') );
    updateDatalist = value.length === 0 || lastFolderInputValue.split('\\').length !== value.split('\\').length;
    
    if(updateDatalist === false){
        for(let i = 0; i < suggestions.length; i++){
            if(suggestions[i].innerText !== value)
                continue;
            
            updateDatalist = true;
            break;
        }
    }

    if(updateDatalist){
        suggestions.forEach(suggestion => {
            suggestion.remove();
        } );

        let req = new XMLHttpRequest();
        req.open('GET', '/folder-suggestion/?input=' + `${value.length === 0 ? '0' : value}`);
        req.addEventListener('loadend', () => {
            let result = req.responseText;
            if(result.length === 0)
                return;

            let resultObj = JSON.parse(result);
            resultObj.forEach(option => {
                let element = document.createElement('option');
                element.setAttribute('value', option);
                element.innerText = option;
                folderDatalist.appendChild(element);

                if(value.length === 0)
                    folderInput.value = ' ';
            } );

        } );

        req.send();
    }
    lastFolderInputValue = value;
}

function getDateString(dateString){
    if(dateString === undefined || dateString.length === 0)
        return '';

    return `${dateString.substring(8, 9 + 1)}/${dateString.substring(5, 6 + 1)}/${dateString.substring(0, 3 + 1)}`;
}

function generateSummary(){
    let checkboxes = Array.from(document.querySelectorAll('.input-checkbox-precision') );

    if(generateButton.disabled === true)
        return;

    generateButton.disabled = true;
    let body = {
        xmlDateFilter: {
            from: getDateString(document.querySelector('#input-date-from-xml').value ),
            to: getDateString(document.querySelector('#input-date-to-xml').value )
        },
        emissionDateFilter: {
            from: getDateString(document.querySelector('#input-date-from-emission').value ),
            to: getDateString(document.querySelector('#input-date-to-emission').value )
        },
        shippingCompany: {
            precise: checkboxes[0].checked,
            value: document.querySelector('#input-shipping-company').value
        },
        volumes: {
            precise: checkboxes[1].checked,
            value: document.querySelector('#input-volumes').value.replaceAll('.', ',')
        },
        weight: {
            precise: checkboxes[2].checked,
            isGrossWeight: document.getElementById('checkbox-import-gross-weight').checked,
            value: document.querySelector('#input-weight').value.replaceAll('.', ',')
        },
        outputFolder: folderInput.value
    }

    let req = new XMLHttpRequest();
    req.open('POST', '/generate');
    req.addEventListener('loadend', () => { onSummaryGenerate(req); } );
    req.send(JSON.stringify(body) );
}

function toggleSummaryRequestFormVisibility(hidden){
    let summaryRequestForm = document.querySelector('#main');
    summaryRequestForm.classList.toggle('hidden', typeof(hidden) === 'boolean' ? hidden : true);
}

function toggleSummaryResultVisibility(hidden){
    let summaryElement = document.querySelector('#summary-result');
    summaryElement.classList.toggle('hidden', typeof(hidden) === 'boolean' ? hidden : true);
}

toggleSummaryRequestFormVisibility(false);
toggleSummaryResultVisibility(true);