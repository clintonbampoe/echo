#!/bin/sh
(while :; do sleep 12h; nginx -s reload; done) &
