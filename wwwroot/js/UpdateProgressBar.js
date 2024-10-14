function ShowProgressBar() {
    let div = document.getElementById("progress");
    div.style.display = "block";
    let divBtn = document.getElementById("button");
    divBtn.style.display = "none";
    UpdateProgress();  // Start updating progress when the progress bar is shown
}

function UpdatePercent(percentage) {
    var progressBar = document.getElementById("progressBar");
    progressBar.style.width = percentage + "%";
    progressBar.innerText = percentage + "%";
    let myText = document.getElementById("text");

    if (percentage >= 20 && percentage <= 39) {
        myText.innerText = "Applying BestJobs ⚙️";
    } else if (percentage >= 40 && percentage <= 59) {
        myText.innerText = "Applying EJobs ⚙️";
    } else if (percentage >= 60 && percentage <= 79) {
        myText.innerText = "Applying Linkedin ⚙️";
    } else if (percentage >= 80 && percentage <= 99) {
        myText.innerText = "Applying Indeed ⚙️";
    } else if (percentage == 100) {
        myText.innerText = "Process Completed ✅";
        div.style.display = "none";
        clearInterval(progressInterval);  // Stop the interval once the process is completed
    }
}

function UpdateProgress() {
    // Call the GetPercentage method every 2 seconds
    progressInterval = setInterval(function () {
        fetch('/AutoApply/GetPercentage')
            .then(response => response.json())
            .then(data => {
                UpdatePercent(data.count);
            })
            .catch(error => console.error('Error fetching percentage:', error));
    }, 2000); // 2000ms = 2 seconds
}

// Assume this is triggered by a button click or similar event
//document.getElementById("startButton").addEventListener("click", function () {
//    ShowProgressBar();
//    // Optionally, trigger the AutoApplySession here if not already running
//});
