FROM ubuntu:19.10

# Configure apt, install packages and tools
RUN apt-get update \
    && apt-get -y install --no-install-recommends apt-utils dialog 2>&1 \
    #
    # Install stuff
    && apt-get -y install wget tar \
    #
    # Clean up
    && apt-get autoremove -y \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/* /go/src

WORKDIR /data

RUN wget https://github.com/gohugoio/hugo/releases/download/v0.59.1/hugo_0.59.1_Linux-64bit.tar.gz -O hugo.tar.gz \
    && tar xvf hugo.tar.gz \
    && mv hugo /bin \
    && rm -rf *

RUN hugo version
